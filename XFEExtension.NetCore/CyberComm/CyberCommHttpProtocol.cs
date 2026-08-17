using System.Buffers;
using System.Globalization;
using System.Net;
using System.Text;

namespace XFEExtension.NetCore.CyberComm;

internal sealed class CyberCommHttpProtocolException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

internal sealed record CyberCommParsedRequest(
    string Method,
    string Target,
    Uri RequestUri,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Headers,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Query,
    ReadOnlyMemory<byte> Body,
    bool KeepAlive,
    bool IsWebSocketRequest,
    string? WebSocketKey);

internal sealed class CyberCommHttpConnectionReader(Stream stream, CyberCommLimitOptions limits)
{
    private readonly byte[] _buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
    private int _start;
    private int _end;
    private bool _disposed;
    public bool RequestStarted { get; private set; }

    public async ValueTask<CyberCommParsedRequest?> ReadRequestAsync(string scheme, CancellationToken cancellationToken)
    {
        RequestStarted = _end > _start;
        var requestLine = await ReadLineAsync(limits.MaxRequestLineBytes, cancellationToken).ConfigureAwait(false);
        if (requestLine is null) return null;
        if (requestLine.Length == 0)
            throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Empty request line");

        var firstSpace = requestLine.IndexOf(' ');
        var secondSpace = firstSpace < 0 ? -1 : requestLine.IndexOf(' ', firstSpace + 1);
        if (firstSpace <= 0 || secondSpace <= firstSpace + 1 || requestLine.IndexOf(' ', secondSpace + 1) >= 0)
            throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Request line must use exactly two single spaces");
        var requestParts = new[] { requestLine[..firstSpace], requestLine[(firstSpace + 1)..secondSpace], requestLine[(secondSpace + 1)..] };
        if (requestParts[2] != "HTTP/1.1")
            throw new CyberCommHttpProtocolException(HttpStatusCode.HttpVersionNotSupported, "Only HTTP/1.1 is supported");
        if (!IsToken(requestParts[0]))
            throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Invalid HTTP method");
        if (!requestParts[1].StartsWith('/') || requestParts[1].Contains('#') ||
            requestParts[1].Any(character => char.IsWhiteSpace(character) || char.IsControl(character)) ||
            !HasValidPercentEncoding(requestParts[1]) || ContainsEncodedDelimiter(requestParts[1]))
            throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Invalid request target");

        var headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var headerBytes = 0;
        while (true)
        {
            var line = await ReadLineAsync(limits.MaxHeaderBytes, cancellationToken).ConfigureAwait(false)
                ?? throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Unexpected end of headers");
            headerBytes += Encoding.ASCII.GetByteCount(line) + 2;
            if (headerBytes > limits.MaxHeaderBytes)
                throw new CyberCommHttpProtocolException((HttpStatusCode)431, "Request headers are too large");
            if (line.Length == 0) break;
            if (char.IsWhiteSpace(line[0]))
                throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Folded headers are not supported");
            var separator = line.IndexOf(':');
            if (separator <= 0 || !IsToken(line.AsSpan(0, separator)))
                throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Invalid header");
            var name = line[..separator];
            var value = line[(separator + 1)..].Trim();
            if (value.Any(character => character is '\r' or '\n' or '\0'))
                throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Invalid header value");
            if (!headers.TryGetValue(name, out var values))
                headers.Add(name, values = []);
            values.Add(value);
        }

        if (!headers.TryGetValue("Host", out var hostValues) || hostValues.Count != 1 || string.IsNullOrWhiteSpace(hostValues[0]))
            throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "A single Host header is required");
        var host = hostValues[0];
        if (host.Any(character => char.IsWhiteSpace(character) || char.IsControl(character)) ||
            !Uri.TryCreate($"{scheme}://{host}", UriKind.Absolute, out var authorityUri) ||
            authorityUri.UserInfo.Length != 0 || authorityUri.AbsolutePath != "/" || authorityUri.Query.Length != 0 || authorityUri.Fragment.Length != 0 ||
            !Uri.TryCreate(authorityUri.GetLeftPart(UriPartial.Authority) + requestParts[1], UriKind.Absolute, out var requestUri))
            throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Invalid request URI");

        var contentLength = ParseContentLength(headers);
        var chunked = ParseTransferEncoding(headers);
        if (contentLength is not null && chunked)
            throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Content-Length and Transfer-Encoding cannot be combined");

        if (headers.TryGetValue("Expect", out var expectValues) &&
            expectValues.Any(value => value.Equals("100-continue", StringComparison.OrdinalIgnoreCase)) &&
            (contentLength > 0 || chunked))
        {
            await stream.WriteAsync("HTTP/1.1 100 Continue\r\n\r\n"u8.ToArray(), cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        byte[] body;
        if (chunked)
            body = await ReadChunkedBodyAsync(cancellationToken).ConfigureAwait(false);
        else if (contentLength is > 0)
        {
            if (contentLength > limits.MaxHttpBodyBytes)
                throw new CyberCommHttpProtocolException(HttpStatusCode.RequestEntityTooLarge, "Request body is too large");
            body = new byte[checked((int)contentLength.Value)];
            await ReadExactlyAsync(body, cancellationToken).ConfigureAwait(false);
        }
        else
            body = [];

        var connectionTokens = GetCommaSeparatedValues(headers, "Connection");
        var keepAlive = !connectionTokens.Contains("close", StringComparer.OrdinalIgnoreCase);
        var upgradeTokens = GetCommaSeparatedValues(headers, "Upgrade");
        var isWebSocket = requestParts[0] == "GET" &&
                          connectionTokens.Contains("upgrade", StringComparer.OrdinalIgnoreCase) &&
                          upgradeTokens.Contains("websocket", StringComparer.OrdinalIgnoreCase);
        var webSocketKey = headers.TryGetValue("Sec-WebSocket-Key", out var keyValues) && keyValues.Count == 1 ? keyValues[0] : null;

        return new(
            requestParts[0],
            requestParts[1],
            requestUri,
            CyberCommHttpCollections.Freeze(headers),
            ParseQuery(requestUri.Query),
            body,
            keepAlive,
            isWebSocket,
            webSocketKey);
    }

    public Stream DetachStream()
    {
        var prefix = _end > _start ? _buffer.AsMemory(_start, _end - _start).ToArray() : [];
        DisposeBuffer();
        return prefix.Length == 0 ? stream : new CyberCommPrebufferedStream(stream, prefix);
    }

    public void DisposeBuffer() => DisposeBufferCore();

    private void DisposeBufferCore()
    {
        if (_disposed) return;
        _disposed = true;
        ArrayPool<byte>.Shared.Return(_buffer);
    }

    private async ValueTask<string?> ReadLineAsync(int maxBytes, CancellationToken cancellationToken)
    {
        var writer = new ArrayBufferWriter<byte>();
        while (true)
        {
            for (var index = _start; index < _end; index++)
            {
                if (_buffer[index] != (byte)'\n') continue;
                var count = index - _start;
                if (count == 0 || _buffer[index - 1] != (byte)'\r')
                    throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "HTTP lines must end with CRLF");
                writer.Write(_buffer.AsSpan(_start, count - 1));
                _start = index + 1;
                if (writer.WrittenCount > maxBytes)
                    throw new CyberCommHttpProtocolException(HttpStatusCode.RequestUriTooLong, "HTTP line is too large");
                ValidateLineBytes(writer.WrittenSpan);
                return Encoding.ASCII.GetString(writer.WrittenSpan);
            }

            if (_end > _start)
            {
                writer.Write(_buffer.AsSpan(_start, _end - _start));
                _start = _end;
                if (writer.WrittenCount > maxBytes)
                    throw new CyberCommHttpProtocolException(HttpStatusCode.RequestUriTooLong, "HTTP line is too large");
            }

            _start = 0;
            _end = await stream.ReadAsync(_buffer, cancellationToken).ConfigureAwait(false);
            if (_end > 0) RequestStarted = true;
            if (_end == 0)
            {
                if (writer.WrittenCount == 0) return null;
                throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Unexpected end of HTTP line");
            }
        }
    }

    private async ValueTask ReadExactlyAsync(Memory<byte> destination, CancellationToken cancellationToken)
    {
        var written = 0;
        while (written < destination.Length)
        {
            if (_start < _end)
            {
                var count = Math.Min(destination.Length - written, _end - _start);
                _buffer.AsMemory(_start, count).CopyTo(destination[written..]);
                _start += count;
                written += count;
                continue;
            }
            _start = 0;
            _end = await stream.ReadAsync(_buffer, cancellationToken).ConfigureAwait(false);
            if (_end == 0)
                throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Unexpected end of request body");
        }
    }

    private async ValueTask<byte[]> ReadChunkedBodyAsync(CancellationToken cancellationToken)
    {
        using var output = new MemoryStream();
        while (true)
        {
            var sizeLine = await ReadLineAsync(128, cancellationToken).ConfigureAwait(false)
                ?? throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Unexpected end of chunked body");
            var extensionSeparator = sizeLine.IndexOf(';');
            var sizeText = extensionSeparator < 0 ? sizeLine : sizeLine[..extensionSeparator];
            if (!long.TryParse(sizeText.Trim(), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var chunkSize) || chunkSize < 0)
                throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Invalid chunk size");
            if (chunkSize > limits.MaxHttpChunkBytes || output.Length + chunkSize > limits.MaxHttpBodyBytes)
                throw new CyberCommHttpProtocolException(HttpStatusCode.RequestEntityTooLarge, "Chunked request body is too large");
            if (chunkSize == 0)
            {
                var trailerBytes = 0;
                while (true)
                {
                    var trailer = await ReadLineAsync(limits.MaxHeaderBytes, cancellationToken).ConfigureAwait(false)
                        ?? throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Unexpected end of trailers");
                    trailerBytes += trailer.Length + 2;
                    if (trailerBytes > limits.MaxHeaderBytes)
                        throw new CyberCommHttpProtocolException((HttpStatusCode)431, "Trailers are too large");
                    if (trailer.Length == 0) break;
                    if (trailer.IndexOf(':') <= 0)
                        throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Invalid trailer");
                }
                return output.ToArray();
            }

            var chunk = new byte[checked((int)chunkSize)];
            await ReadExactlyAsync(chunk, cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(chunk, cancellationToken).ConfigureAwait(false);
            var terminator = new byte[2];
            await ReadExactlyAsync(terminator, cancellationToken).ConfigureAwait(false);
            if (terminator[0] != '\r' || terminator[1] != '\n')
                throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Invalid chunk terminator");
        }
    }

    private static long? ParseContentLength(Dictionary<string, List<string>> headers)
    {
        if (!headers.TryGetValue("Content-Length", out var values)) return null;
        var allValues = values.SelectMany(value => value.Split(',')).Select(value => value.Trim()).ToArray();
        if (allValues.Length == 0 || allValues.Any(value => !long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out _)))
            throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Invalid Content-Length");
        var parsed = allValues.Select(value => long.Parse(value, CultureInfo.InvariantCulture)).ToArray();
        if (parsed.Any(value => value < 0) || parsed.Distinct().Count() != 1)
            throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "Conflicting Content-Length headers");
        return parsed[0];
    }

    private static bool ParseTransferEncoding(Dictionary<string, List<string>> headers)
    {
        var values = GetCommaSeparatedValues(headers, "Transfer-Encoding");
        if (values.Count == 0) return false;
        if (values.Count != 1 || !values[0].Equals("chunked", StringComparison.OrdinalIgnoreCase))
            throw new CyberCommHttpProtocolException(HttpStatusCode.NotImplemented, "Only chunked Transfer-Encoding is supported");
        return true;
    }

    private static List<string> GetCommaSeparatedValues(Dictionary<string, List<string>> headers, string name)
        => headers.TryGetValue(name, out var values)
            ? values.SelectMany(value => value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)).ToList()
            : [];

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> ParseQuery(string query)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = part.IndexOf('=');
            var key = Uri.UnescapeDataString(separator < 0 ? part : part[..separator]);
            var value = Uri.UnescapeDataString(separator < 0 ? string.Empty : part[(separator + 1)..]);
            if (!result.TryGetValue(key, out var values)) result.Add(key, values = []);
            values.Add(value);
        }
        return CyberCommHttpCollections.Freeze(result);
    }

    private static bool IsToken(string value) => IsToken(value.AsSpan());

    private static bool IsToken(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty) return false;
        foreach (var character in value)
        {
            if (!(char.IsAsciiLetterOrDigit(character) || character is '!' or '#' or '$' or '%' or '&' or '\'' or '*' or '+' or '-' or '.' or '^' or '_' or '`' or '|' or '~'))
                return false;
        }
        return true;
    }

    private static void ValidateLineBytes(ReadOnlySpan<byte> bytes)
    {
        foreach (var value in bytes)
        {
            if (value > 0x7f || value < 0x20 && value != (byte)'\t' || value == 0x7f)
                throw new CyberCommHttpProtocolException(HttpStatusCode.BadRequest, "HTTP line contains invalid bytes");
        }
    }

    private static bool HasValidPercentEncoding(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != '%') continue;
            if (index + 2 >= value.Length || !Uri.IsHexDigit(value[index + 1]) || !Uri.IsHexDigit(value[index + 2])) return false;
            index += 2;
        }
        return true;
    }

    private static bool ContainsEncodedDelimiter(string value) =>
        value.Contains("%2f", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("%5c", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("%0d", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("%0a", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("%00", StringComparison.OrdinalIgnoreCase);
}

internal sealed class CyberCommPrebufferedStream(Stream inner, byte[] prefix) : Stream
{
    private int _position;
    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => inner.CanWrite;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override void Flush() => inner.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) => inner.FlushAsync(cancellationToken);
    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_position < prefix.Length)
        {
            var copied = Math.Min(count, prefix.Length - _position);
            prefix.AsSpan(_position, copied).CopyTo(buffer.AsSpan(offset, copied));
            _position += copied;
            return copied;
        }
        return inner.Read(buffer, offset, count);
    }
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_position < prefix.Length)
        {
            var copied = Math.Min(buffer.Length, prefix.Length - _position);
            prefix.AsMemory(_position, copied).CopyTo(buffer);
            _position += copied;
            return copied;
        }
        return await inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
    }
    public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => inner.WriteAsync(buffer, cancellationToken);
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}
