using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using XFEExtension.NetCore.Exceptions;

namespace XFEExtension.NetCore.CyberComm;

/// <summary>
/// 基于 Socket、HTTP/1.1 和 WebSocket 的跨平台 CyberComm 服务器。
/// </summary>
public sealed class CyberCommServer : IAsyncDisposable
{
    private HttpListener? _legacyServer;
    private readonly SemaphoreSlim _lifecycleGate = new(1, 1);
    private readonly List<ListenerRegistration> _listeners = [];
    private readonly ConcurrentDictionary<long, ActiveConnection> _connections = new();
    private readonly ConcurrentDictionary<string, int> _connectionsPerIp = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _runSource;
    private SemaphoreSlim? _connectionSlots;
    private TaskCompletionSource _runCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private long _connectionId;
    private X509Certificate2? _certificate;
    private bool _ownsCertificate;
    private CyberCommServerOptions? _configuredOptions;
    private static readonly HashSet<string> s_supportedMethods = new(StringComparer.Ordinal)
    {
        "GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS", "HEAD"
    };

    public string[] ServerUrlArray { get; set; } = [];
    public int BufferLength { get; set; } = 16 * 1024;
    public bool ServerRunning => State == CyberCommServerState.Running;
    public bool AutoReceiveCompletedMessage { get; set; } = true;
    public bool ReadHttpRequestBody { get; set; } = true;
    public CyberCommServerState State { get; private set; } = CyberCommServerState.Created;

    [Obsolete("请使用 WebSocketMessageHandler")]
    public event EventHandler<CyberCommServerEventArgs>? MessageReceived;
    [Obsolete("请使用 WebSocketConnectedHandler")]
    public event EventHandler<CyberCommServerEventArgs>? ClientConnected;
    [Obsolete("请使用 StartedHandler")]
    public event EventHandler? ServerStarted;
    [Obsolete("请使用 WebSocketClosedHandler")]
    public event EventHandler<CyberCommServerEventArgs>? ConnectionClosed;
    [Obsolete("请使用 HttpRequestHandler")]
    public event EventHandler<CyberCommRequestEventArgs>? RequestReceived;

    public Func<CyberCommHttpRequestContext, CancellationToken, ValueTask>? HttpRequestHandler { get; set; }
    public Func<CyberCommServerEventArgs, CancellationToken, ValueTask>? WebSocketMessageHandler { get; set; }
    public Func<CyberCommServerEventArgs, CancellationToken, ValueTask>? WebSocketConnectedHandler { get; set; }
    public Func<CyberCommServerEventArgs, CancellationToken, ValueTask>? WebSocketClosedHandler { get; set; }
    public Func<CancellationToken, ValueTask>? StartedHandler { get; set; }
    /// <summary>接收后台传输异常；回调异常会被隔离。</summary>
    public Action<Exception>? ErrorHandler { get; set; }

    /// <summary>
    /// 仅为二进制兼容保留。新 Socket 模式不使用 HttpListener。
    /// </summary>
    [Obsolete("CyberCommServer 已使用跨平台 Socket 引擎")]
    public HttpListener Server { get => _legacyServer ??= new(); set => _legacyServer = value; }

    public CyberCommServer()
    {
    }

    public CyberCommServer(CyberCommServerOptions options)
    {
        _configuredOptions = options ?? throw new ArgumentNullException(nameof(options));
        ServerUrlArray = [.. options.ListenUrls];
        AutoReceiveCompletedMessage = options.AssembleWebSocketMessages;
        ReadHttpRequestBody = options.ReadHttpRequestBody;
    }

    public CyberCommServer(params int[] listenPorts)
        : this(listenPorts.Select(port => $"http://*:{port}/").ToArray())
    {
    }

    public CyberCommServer(params string[] serverUrlArray)
    {
        ServerUrlArray = serverUrlArray;
    }

    public CyberCommServer(bool autoReceiveCompletedMessage = true, params string[] serverUrlArray)
        : this(serverUrlArray)
    {
        AutoReceiveCompletedMessage = autoReceiveCompletedMessage;
    }

    /// <summary>
    /// 启动监听器。该方法在监听完成后返回，使用 <see cref="RunAsync"/> 等待服务器停止。
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (State == CyberCommServerState.Running) return;
            if (State is CyberCommServerState.Starting or CyberCommServerState.Stopping)
                throw new InvalidOperationException($"服务器当前状态不允许启动：{State}");

            State = CyberCommServerState.Starting;
            try
            {
                var options = GetEffectiveOptions();
                var endpoints = options.ValidateAndGetEndpoints();
                _certificate = endpoints.Any(endpoint => endpoint.UseTls) ? options.Tls!.LoadCertificate() : null;
                _ownsCertificate = _certificate is not null && options.Tls!.Certificate is null;
                _runSource = new CancellationTokenSource();
                _connectionSlots = new SemaphoreSlim(options.Limits.MaxConnections, options.Limits.MaxConnections);
                _runCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);

                foreach (var endpoint in endpoints)
                {
                    var listener = new TcpListener(endpoint.Address, endpoint.Port);
                    listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    listener.Start();
                    var acceptTask = AcceptLoopAsync(listener, endpoint, options, _runSource.Token);
                    _listeners.Add(new(listener, acceptTask));
                }

                State = CyberCommServerState.Running;
                await InvokeStartedAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                foreach (var registration in _listeners) registration.Listener.Stop();
                _listeners.Clear();
                _runSource?.Cancel();
                _runSource?.Dispose();
                _runSource = null;
                _connectionSlots?.Dispose();
                _connectionSlots = null;
                if (_ownsCertificate) _certificate?.Dispose();
                _certificate = null;
                _ownsCertificate = false;
                State = CyberCommServerState.Faulted;
                throw;
            }
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    /// <summary>
    /// 等待服务器停止。
    /// </summary>
    public Task RunAsync(CancellationToken cancellationToken = default)
        => _runCompletion.Task.WaitAsync(cancellationToken);

    /// <summary>
    /// 停止监听并在限定时间内关闭活动连接。
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (State is CyberCommServerState.Created or CyberCommServerState.Stopped)
            {
                State = CyberCommServerState.Stopped;
                _runCompletion.TrySetResult();
                return;
            }
            if (State == CyberCommServerState.Stopping) return;

            State = CyberCommServerState.Stopping;
            var closeTimeout = GetEffectiveOptions().Limits.CloseTimeout;
            foreach (var registration in _listeners) registration.Listener.Stop();

            var acceptTasks = _listeners.Select(item => item.AcceptTask).ToArray();
            if (acceptTasks.Length > 0)
            {
                try { await Task.WhenAll(acceptTasks).ConfigureAwait(false); }
                catch (Exception) { }
            }

            var webSocketCloseTasks = new List<Task>();
            foreach (var connection in _connections.Values)
            {
                if (!connection.IsWebSocket)
                {
                    if (!connection.IsHandlingRequest) connection.Client.Dispose();
                    continue;
                }

                var peer = connection.Peer;
                if (peer is null)
                    connection.Client.Dispose();
                else
                    webSocketCloseTasks.Add(peer.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server stopping", cancellationToken).AsTask());
            }

            var connectionTasks = _connections.Values.Select(item => item.Task).ToArray();
            if (connectionTasks.Length > 0)
            {
                using var gracefulTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                if (closeTimeout != Timeout.InfiniteTimeSpan) gracefulTimeout.CancelAfter(closeTimeout);
                try { await Task.WhenAll(connectionTasks.Concat(webSocketCloseTasks)).WaitAsync(gracefulTimeout.Token).ConfigureAwait(false); }
                catch (OperationCanceledException) { }
                catch (Exception) { }
            }

            _runSource?.Cancel();
            foreach (var connection in _connections.Values) connection.Client.Dispose();
            if (connectionTasks.Length > 0)
            {
                try { await Task.WhenAll(connectionTasks).ConfigureAwait(false); }
                catch (Exception) { }
            }

            _listeners.Clear();
            _connections.Clear();
            _connectionsPerIp.Clear();
            _connectionSlots?.Dispose();
            _connectionSlots = null;
            _runSource?.Dispose();
            _runSource = null;
            if (_ownsCertificate) _certificate?.Dispose();
            _certificate = null;
            _ownsCertificate = false;
            State = CyberCommServerState.Stopped;
            _runCompletion.TrySetResult();
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    [Obsolete("请使用 StartAsync 和 RunAsync")]
    public async Task StartCyberCommServer()
    {
        try
        {
            await StartAsync().ConfigureAwait(false);
            await RunAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not XFECyberCommException)
        {
            throw new XFECyberCommException("启动服务器时发生异常", ex);
        }
    }

    [Obsolete("请使用 StopAsync")]
    public void StopCyberCommServer() => StopAsync().GetAwaiter().GetResult();

    private CyberCommServerOptions GetEffectiveOptions()
    {
        if (_configuredOptions is not null)
            return _configuredOptions;
        return new()
        {
            ListenUrls = ServerUrlArray,
            AssembleWebSocketMessages = AutoReceiveCompletedMessage,
            ReadHttpRequestBody = ReadHttpRequestBody
        };
    }

    private async Task AcceptLoopAsync(TcpListener listener, CyberCommListenEndpoint endpoint, CyberCommServerOptions options, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _connectionSlots!.WaitAsync(cancellationToken).ConfigureAwait(false);
                TcpClient client;
                try
                {
                    client = await listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    _connectionSlots.Release();
                    throw;
                }

                client.NoDelay = true;
                var ip = ((IPEndPoint?)client.Client.RemoteEndPoint)?.Address.ToString() ?? "unknown";
                var ipCount = _connectionsPerIp.AddOrUpdate(ip, 1, static (_, count) => count + 1);
                if (ipCount > options.Limits.MaxConnectionsPerIp)
                {
                    DecrementIp(ip);
                    client.Dispose();
                    _connectionSlots.Release();
                    continue;
                }

                var id = Interlocked.Increment(ref _connectionId);
                var connection = new ActiveConnection(client);
                _connections[id] = connection;
                connection.SetTask(RunTrackedConnectionAsync(id, client, endpoint, options, ip, connection, cancellationToken));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (SocketException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            if (State == CyberCommServerState.Running)
            {
                State = CyberCommServerState.Faulted;
                _runCompletion.TrySetException(ex);
                _runSource?.Cancel();
            }
        }
    }

    private async Task RunTrackedConnectionAsync(long id, TcpClient client, CyberCommListenEndpoint endpoint,
        CyberCommServerOptions options, string ip, ActiveConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            await ProcessConnectionAsync(client, endpoint, options, ip, connection, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            InvokeErrorSafely(ex);
        }
        finally
        {
            _connections.TryRemove(id, out _);
            DecrementIp(ip);
            _connectionSlots!.Release();
        }
    }

    private async Task ProcessConnectionAsync(TcpClient client, CyberCommListenEndpoint endpoint, CyberCommServerOptions options,
        string clientIp, ActiveConnection connection, CancellationToken serverToken)
    {
        using (client)
        {
            var localPort = ((IPEndPoint?)client.Client.LocalEndPoint)?.Port ?? endpoint.Port;
            Stream stream = client.GetStream();
            if (endpoint.UseTls)
            {
                var sslStream = new SslStream(stream, false);
                await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                {
                    ServerCertificate = _certificate,
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    ClientCertificateRequired = false
                }, serverToken).ConfigureAwait(false);
                stream = sslStream;
            }

            await using (stream.ConfigureAwait(false))
            {
                var reader = new CyberCommHttpConnectionReader(stream, options.Limits);
                try
                {
                    while (!serverToken.IsCancellationRequested)
                    {
                        CyberCommParsedRequest? request;
                        using var idleSource = CancellationTokenSource.CreateLinkedTokenSource(serverToken);
                        var receiveTimeout = MinTimeout(options.Limits.IdleTimeout, options.Limits.ReceiveTimeout);
                        if (receiveTimeout != Timeout.InfiniteTimeSpan)
                            idleSource.CancelAfter(receiveTimeout);
                        try
                        {
                            request = await reader.ReadRequestAsync(endpoint.UseTls ? "https" : "http", idleSource.Token).ConfigureAwait(false);
                        }
                        catch (CyberCommHttpProtocolException ex)
                        {
                            await WriteSimpleErrorAsync(stream, ex.StatusCode, ex.Message, false,
                                options.Limits.SendTimeout, serverToken).ConfigureAwait(false);
                            break;
                        }
                        catch (OperationCanceledException) when (idleSource.IsCancellationRequested)
                        {
                            if (!serverToken.IsCancellationRequested && reader.RequestStarted)
                                await WriteSimpleErrorAsync(stream, HttpStatusCode.RequestTimeout, "Request Timeout", false,
                                    options.Limits.SendTimeout, serverToken).ConfigureAwait(false);
                            break;
                        }

                        if (request is null) break;
                        if (request.IsWebSocketRequest)
                        {
                            connection.IsWebSocket = true;
                            var webSocketStream = reader.DetachStream();
                            await ProcessWebSocketAsync(webSocketStream, request, options, clientIp, localPort, connection, serverToken).ConfigureAwait(false);
                            return;
                        }

                        connection.IsHandlingRequest = true;
                        try
                        {
                            await ProcessHttpRequestAsync(stream, request, options, clientIp, localPort, serverToken).ConfigureAwait(false);
                        }
                        finally
                        {
                            connection.IsHandlingRequest = false;
                        }
                        if (!request.KeepAlive || State == CyberCommServerState.Stopping) break;
                    }
                }
                catch (IOException)
                {
                }
                catch (SocketException)
                {
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    reader.DisposeBuffer();
                }
            }
        }
    }

    private async Task ProcessHttpRequestAsync(Stream stream, CyberCommParsedRequest request, CyberCommServerOptions options,
        string clientIp, int localPort, CancellationToken serverToken)
    {
        var body = options.ReadHttpRequestBody ? request.Body : ReadOnlyMemory<byte>.Empty;
        var context = new CyberCommHttpRequestContext(request.RequestUri, request.Method, request.Headers, request.Query,
            body, clientIp, localPort, Guid.NewGuid().ToString("N"));
        context.Response.Headers["X-Correlation-Id"] = context.CorrelationId;
        using var handlerSource = CancellationTokenSource.CreateLinkedTokenSource(serverToken);
        if (options.Limits.HandlerTimeout != Timeout.InfiniteTimeSpan)
            handlerSource.CancelAfter(options.Limits.HandlerTimeout);

        try
        {
            if (!s_supportedMethods.Contains(request.Method))
            {
                context.Response.Headers["Allow"] = string.Join(", ", s_supportedMethods);
                await context.Response.WriteTextAsync("Method Not Allowed", HttpStatusCode.MethodNotAllowed).ConfigureAwait(false);
                context.Response.Complete();
            }
            else if (HttpRequestHandler is not null)
                await HttpRequestHandler(context, handlerSource.Token).AsTask().WaitAsync(handlerSource.Token).ConfigureAwait(false);
            else if (RequestReceived is not null)
                InvokeSafely(RequestReceived, new CyberCommRequestEventArgsImpl(context));
        }
        catch (OperationCanceledException) when (serverToken.IsCancellationRequested)
        {
            return;
        }
        catch (OperationCanceledException) when (!serverToken.IsCancellationRequested)
        {
            if (!context.Response.IsCompleted)
            {
                await context.Response.WriteTextAsync("Gateway Timeout", HttpStatusCode.GatewayTimeout).ConfigureAwait(false);
                context.Response.Complete();
            }
        }
        catch (Exception ex)
        {
            InvokeErrorSafely(ex);
            if (!context.Response.IsCompleted)
            {
                await context.Response.WriteTextAsync("Internal Server Error", HttpStatusCode.InternalServerError).ConfigureAwait(false);
                context.Response.Complete();
            }
        }

        context.Response.EnsureCompleted(HttpStatusCode.NotFound);
        using var sendSource = CancellationTokenSource.CreateLinkedTokenSource(serverToken);
        if (options.Limits.SendTimeout != Timeout.InfiniteTimeSpan)
            sendSource.CancelAfter(options.Limits.SendTimeout);
        await WriteResponseAsync(stream, request.Method, context.Response,
            request.KeepAlive && State != CyberCommServerState.Stopping, sendSource.Token).ConfigureAwait(false);
    }

    private async Task ProcessWebSocketAsync(Stream stream, CyberCommParsedRequest request, CyberCommServerOptions options,
        string clientIp, int localPort, ActiveConnection connection, CancellationToken serverToken)
    {
        if (!ValidateWebSocketRequest(request, out var key))
        {
            await WriteSimpleErrorAsync(stream, (HttpStatusCode)426, "WebSocket version 13 is required", false,
                options.Limits.SendTimeout, serverToken).ConfigureAwait(false);
            return;
        }

        var acceptBytes = SHA1.HashData(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
        var response = $"HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: {Convert.ToBase64String(acceptBytes)}\r\n\r\n";
        await stream.WriteAsync(Encoding.ASCII.GetBytes(response), serverToken).ConfigureAwait(false);
        await stream.FlushAsync(serverToken).ConfigureAwait(false);

        using var webSocket = WebSocket.CreateFromStream(stream, true, null, options.WebSocketKeepAliveInterval);
        await using var peer = new CyberCommWebSocketPeer(webSocket, options.Limits);
        connection.Peer = peer;
        var headers = ToNameValueCollection(request.Headers);
        var connectedArgs = new CyberCommServerEventArgsImpl(request.RequestUri, webSocket, string.Empty, clientIp, headers, true, localPort).WithTransport(peer);
        await InvokeWebSocketHandlerAsync(WebSocketConnectedHandler, ClientConnected, connectedArgs, serverToken).ConfigureAwait(false);

        try
        {
            await ReceiveWebSocketMessagesAsync(webSocket, peer, request.RequestUri, headers, clientIp, localPort, options, serverToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (serverToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            var errorArgs = new CyberCommServerEventArgsImpl(request.RequestUri, webSocket,
                new XFECyberCommException("与客户端通讯期间发生异常", ex), clientIp, headers, localPort).WithTransport(peer);
            await InvokeWebSocketHandlerAsync(WebSocketMessageHandler, MessageReceived, errorArgs, serverToken).ConfigureAwait(false);
        }
        finally
        {
            if (webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await peer.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection Closed", serverToken).ConfigureAwait(false);
            var closedArgs = new CyberCommServerEventArgsImpl(request.RequestUri, webSocket, string.Empty, clientIp, headers, true, localPort).WithTransport(peer);
            await InvokeWebSocketHandlerAsync(WebSocketClosedHandler, ConnectionClosed, closedArgs, CancellationToken.None).ConfigureAwait(false);
            connection.Peer = null;
        }
    }

    private async Task ReceiveWebSocketMessagesAsync(WebSocket webSocket, CyberCommWebSocketPeer peer, Uri requestUri,
        NameValueCollection headers, string clientIp, int localPort, CyberCommServerOptions options,
        CancellationToken cancellationToken)
    {
        var receiveSize = Math.Clamp(BufferLength, 1024, options.Limits.MaxWebSocketFrameBytes);
        var buffer = new byte[receiveSize];
        while (webSocket.State is WebSocketState.Open or WebSocketState.CloseSent)
        {
            using var receiveSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (options.Limits.ReceiveTimeout != Timeout.InfiniteTimeSpan)
                receiveSource.CancelAfter(options.Limits.ReceiveTimeout);
            var result = await webSocket.ReceiveAsync(buffer, receiveSource.Token).ConfigureAwait(false);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await peer.CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription ?? string.Empty, cancellationToken).ConfigureAwait(false);
                break;
            }

            if (!options.AssembleWebSocketMessages)
            {
                await PublishWebSocketMessageAsync(webSocket, peer, requestUri, headers, clientIp, localPort, result.MessageType,
                    buffer.AsMemory(0, result.Count).ToArray(), result.EndOfMessage, false, cancellationToken).ConfigureAwait(false);
                continue;
            }

            using var message = new MemoryStream();
            await message.WriteAsync(buffer.AsMemory(0, result.Count), cancellationToken).ConfigureAwait(false);
            var messageType = result.MessageType;
            while (!result.EndOfMessage)
            {
                if (message.Length > options.Limits.MaxWebSocketMessageBytes)
                {
                    await peer.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Message is too large", cancellationToken).ConfigureAwait(false);
                    return;
                }
                result = await webSocket.ReceiveAsync(buffer, receiveSource.Token).ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Close) break;
                if (result.MessageType != messageType)
                    throw new WebSocketException(WebSocketError.Faulted, "Message type changed during fragmentation");
                await message.WriteAsync(buffer.AsMemory(0, result.Count), cancellationToken).ConfigureAwait(false);
            }
            if (message.Length > options.Limits.MaxWebSocketMessageBytes)
            {
                await peer.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Message is too large", cancellationToken).ConfigureAwait(false);
                return;
            }
            if (result.MessageType == WebSocketMessageType.Close) break;
            await PublishWebSocketMessageAsync(webSocket, peer, requestUri, headers, clientIp, localPort,
                messageType, message.ToArray(), true, true, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task PublishWebSocketMessageAsync(WebSocket webSocket, CyberCommWebSocketPeer peer, Uri requestUri,
        NameValueCollection headers, string clientIp, int localPort, WebSocketMessageType messageType, byte[] payload,
        bool endOfMessage, bool validateUtf8, CancellationToken cancellationToken)
    {
        CyberCommServerEventArgs args;
        if (messageType == WebSocketMessageType.Text)
        {
            string text;
            try { text = validateUtf8 ? new UTF8Encoding(false, true).GetString(payload) : Encoding.UTF8.GetString(payload); }
            catch (DecoderFallbackException)
            {
                await peer.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid UTF-8", cancellationToken).ConfigureAwait(false);
                return;
            }
            args = new CyberCommServerEventArgsImpl(requestUri, webSocket, text, clientIp, headers, endOfMessage, localPort);
        }
        else if (messageType == WebSocketMessageType.Binary)
            args = new CyberCommServerEventArgsImpl(requestUri, webSocket, payload, clientIp, headers, endOfMessage, localPort);
        else
            throw new ArgumentOutOfRangeException(nameof(messageType));
        await InvokeWebSocketHandlerAsync(WebSocketMessageHandler, MessageReceived, args.WithTransport(peer), cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask InvokeWebSocketHandlerAsync(
        Func<CyberCommServerEventArgs, CancellationToken, ValueTask>? handler,
        EventHandler<CyberCommServerEventArgs>? legacyEvent,
        CyberCommServerEventArgs args,
        CancellationToken cancellationToken)
    {
        if (handler is not null)
        {
            try { await handler(args, cancellationToken).ConfigureAwait(false); }
            catch (Exception ex) { InvokeErrorSafely(ex); }
        }
        InvokeSafely(legacyEvent, args);
    }

    private static bool ValidateWebSocketRequest(CyberCommParsedRequest request, out string key)
    {
        key = request.WebSocketKey ?? string.Empty;
        if (!request.Headers.TryGetValue("Sec-WebSocket-Version", out var versions) || versions.Count != 1 || versions[0] != "13")
            return false;
        try { return Convert.FromBase64String(key).Length == 16; }
        catch (FormatException) { return false; }
    }

    private static async Task WriteResponseAsync(Stream stream, string method, CyberCommHttpResponse response, bool keepAlive, CancellationToken cancellationToken)
    {
        var body = response.Body;
        var contentType = IsSafeHeader("Content-Type", response.ContentType)
            ? response.ContentType
            : "application/octet-stream";
        var builder = new StringBuilder()
            .Append("HTTP/1.1 ").Append((int)response.StatusCode).Append(' ').Append(GetReasonPhrase(response.StatusCode)).Append("\r\n")
            .Append("Content-Type: ").Append(contentType).Append("\r\n")
            .Append("Content-Length: ").Append(body.Length).Append("\r\n")
            .Append("Connection: ").Append(keepAlive ? "keep-alive" : "close").Append("\r\n");
        foreach (var (name, value) in response.Headers)
        {
            if (!IsSafeHeader(name, value) || name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Connection", StringComparison.OrdinalIgnoreCase) || name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                continue;
            builder.Append(name).Append(": ").Append(value).Append("\r\n");
        }
        builder.Append("\r\n");
        await stream.WriteAsync(Encoding.ASCII.GetBytes(builder.ToString()), cancellationToken).ConfigureAwait(false);
        if (!method.Equals("HEAD", StringComparison.OrdinalIgnoreCase) && !body.IsEmpty)
            await stream.WriteAsync(body, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteSimpleErrorAsync(Stream stream, HttpStatusCode statusCode, string message, bool keepAlive,
        TimeSpan sendTimeout, CancellationToken cancellationToken)
    {
        var response = new CyberCommHttpResponse();
        await response.WriteTextAsync(message, statusCode).ConfigureAwait(false);
        response.Complete();
        using var sendSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (sendTimeout != Timeout.InfiniteTimeSpan) sendSource.CancelAfter(sendTimeout);
        await WriteResponseAsync(stream, "GET", response, keepAlive, sendSource.Token).ConfigureAwait(false);
    }

    private static TimeSpan MinTimeout(TimeSpan left, TimeSpan right)
    {
        if (left == Timeout.InfiniteTimeSpan) return right;
        if (right == Timeout.InfiniteTimeSpan) return left;
        return left <= right ? left : right;
    }

    private static string GetReasonPhrase(HttpStatusCode statusCode) => (int)statusCode switch
    {
        101 => "Switching Protocols",
        200 => "OK",
        201 => "Created",
        204 => "No Content",
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        405 => "Method Not Allowed",
        408 => "Request Timeout",
        409 => "Conflict",
        413 => "Content Too Large",
        422 => "Unprocessable Content",
        426 => "Upgrade Required",
        429 => "Too Many Requests",
        431 => "Request Header Fields Too Large",
        500 => "Internal Server Error",
        501 => "Not Implemented",
        503 => "Service Unavailable",
        504 => "Gateway Timeout",
        505 => "HTTP Version Not Supported",
        _ => statusCode.ToString()
    };

    private static bool IsSafeHeader(string name, string value)
        => name.Length > 0 && !name.Any(char.IsWhiteSpace) && !name.Contains(':') &&
           !name.Contains('\r') && !name.Contains('\n') && !value.Contains('\r') && !value.Contains('\n');

    private static NameValueCollection ToNameValueCollection(IReadOnlyDictionary<string, IReadOnlyList<string>> source)
    {
        var result = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, values) in source)
            foreach (var value in values)
                result.Add(name, value);
        return result;
    }

    private void DecrementIp(string ip)
    {
        while (_connectionsPerIp.TryGetValue(ip, out var count))
        {
            if (count <= 1)
            {
                if (_connectionsPerIp.TryRemove(new KeyValuePair<string, int>(ip, count))) return;
            }
            else if (_connectionsPerIp.TryUpdate(ip, count - 1, count)) return;
        }
    }

    private void InvokeSafely<TEventArgs>(EventHandler<TEventArgs>? handler, TEventArgs args)
    {
        if (handler is null) return;
        foreach (EventHandler<TEventArgs> subscriber in handler.GetInvocationList())
        {
            try { subscriber(this, args); } catch (Exception ex) { InvokeErrorSafely(ex); }
        }
    }

    private void InvokeSafely(EventHandler? handler, EventArgs args)
    {
        if (handler is null) return;
        foreach (EventHandler subscriber in handler.GetInvocationList())
        {
            try { subscriber(this, args); } catch (Exception ex) { InvokeErrorSafely(ex); }
        }
    }

    private void InvokeErrorSafely(Exception exception)
    {
        try { ErrorHandler?.Invoke(exception); }
        catch (Exception) { }
    }

    private async ValueTask InvokeStartedAsync(CancellationToken cancellationToken)
    {
        if (StartedHandler is not null)
        {
            try { await StartedHandler(cancellationToken).ConfigureAwait(false); }
            catch (Exception ex) { InvokeErrorSafely(ex); }
        }
#pragma warning disable CS0618 // 单版本兼容适配器
        InvokeSafely(ServerStarted, EventArgs.Empty);
#pragma warning restore CS0618
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _lifecycleGate.Dispose();
        _legacyServer?.Close();
    }

    private sealed record ListenerRegistration(TcpListener Listener, Task AcceptTask);
    private sealed class ActiveConnection(TcpClient client)
    {
        private readonly TaskCompletionSource<Task> _taskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _isHandlingRequest;
        private int _isWebSocket;
        private CyberCommWebSocketPeer? _peer;

        public TcpClient Client { get; } = client;
        public Task Task => AwaitAssignedTaskAsync();
        public bool IsHandlingRequest
        {
            get => Volatile.Read(ref _isHandlingRequest) != 0;
            set => Volatile.Write(ref _isHandlingRequest, value ? 1 : 0);
        }
        public bool IsWebSocket
        {
            get => Volatile.Read(ref _isWebSocket) != 0;
            set => Volatile.Write(ref _isWebSocket, value ? 1 : 0);
        }
        public CyberCommWebSocketPeer? Peer
        {
            get => Volatile.Read(ref _peer);
            set => Volatile.Write(ref _peer, value);
        }

        public void SetTask(Task task) => _taskSource.TrySetResult(task);

        private async Task AwaitAssignedTaskAsync()
        {
            var task = await _taskSource.Task.ConfigureAwait(false);
            await task.ConfigureAwait(false);
        }
    }
}
