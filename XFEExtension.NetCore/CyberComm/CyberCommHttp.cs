using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace XFEExtension.NetCore.CyberComm;

/// <summary>
/// 与具体 HTTP 监听实现无关的请求上下文。
/// </summary>
public sealed class CyberCommHttpRequestContext
{
    internal CyberCommHttpRequestContext(
        Uri requestUri,
        string method,
        IReadOnlyDictionary<string, IReadOnlyList<string>> headers,
        IReadOnlyDictionary<string, IReadOnlyList<string>> query,
        ReadOnlyMemory<byte> body,
        string clientIp,
        int localPort,
        string correlationId)
    {
        RequestUri = requestUri;
        Method = method;
        Headers = headers;
        Query = query;
        Body = body;
        ClientIp = clientIp;
        LocalPort = localPort;
        CorrelationId = correlationId;
    }

    public Uri RequestUri { get; }
    public string Method { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Headers { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Query { get; }
    public ReadOnlyMemory<byte> Body { get; }
    public string RequestBody => Encoding.UTF8.GetString(Body.Span);
    public string ClientIp { get; }
    /// <summary>实际接受当前连接的本地监听端口，不受 Host 请求头或反向代理公开端口影响。</summary>
    public int LocalPort { get; }
    public string CorrelationId { get; }
    public CyberCommHttpResponse Response { get; } = new();
}

/// <summary>
/// CyberComm 缓冲式 HTTP 响应。响应由服务端统一写出并关闭或复用连接。
/// </summary>
public sealed class CyberCommHttpResponse
{
    private readonly object _syncRoot = new();
    private byte[] _body = [];

    public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.OK;
    public string ContentType { get; private set; } = "text/plain; charset=utf-8";
    public IDictionary<string, string> Headers { get; } = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public bool IsCompleted { get; private set; }
    internal ReadOnlyMemory<byte> Body => _body;

    public ValueTask WriteTextAsync(string text, HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = "text/plain; charset=utf-8")
        => WriteAsync(Encoding.UTF8.GetBytes(text ?? string.Empty), statusCode, contentType);

    public ValueTask WriteAsync(ReadOnlyMemory<byte> body, HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = "application/octet-stream")
    {
        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(IsCompleted, this);
            StatusCode = statusCode;
            ContentType = contentType;
            _body = body.ToArray();
        }
        return ValueTask.CompletedTask;
    }

    public void Complete(HttpStatusCode? statusCode = null)
    {
        lock (_syncRoot)
        {
            if (IsCompleted) return;
            if (statusCode is not null)
                StatusCode = statusCode.Value;
            IsCompleted = true;
        }
    }

    internal void EnsureCompleted(HttpStatusCode fallbackStatus)
    {
        lock (_syncRoot)
        {
            if (IsCompleted) return;
            if (_body.Length == 0)
            {
                StatusCode = fallbackStatus;
                _body = Encoding.UTF8.GetBytes(fallbackStatus == HttpStatusCode.NotFound ? "Not Found" : "No response was produced");
            }
            IsCompleted = true;
        }
    }
}

/// <summary>
/// CyberComm HTTP 异步处理器。
/// </summary>
public interface ICyberCommHttpHandler
{
    ValueTask HandleAsync(CyberCommHttpRequestContext context, CancellationToken cancellationToken);
}

internal static class CyberCommHttpCollections
{
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> Freeze(Dictionary<string, List<string>> source)
        => new ReadOnlyDictionary<string, IReadOnlyList<string>>(
            source.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<string>)pair.Value.AsReadOnly(), source.Comparer));
}
