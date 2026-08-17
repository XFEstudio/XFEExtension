using System.Net.WebSockets;
using System.Text;
using XFEExtension.NetCore.Exceptions;

namespace XFEExtension.NetCore.CyberComm;

/// <summary>
/// 具有可取消生命周期、串行发送和安全重连的 CyberComm WebSocket 客户端。
/// </summary>
public sealed class CyberCommClient : IAsyncDisposable
{
    private readonly SemaphoreSlim _lifecycleGate = new(1, 1);
    private CyberCommClientOptions? _configuredOptions;
    private CancellationTokenSource? _lifetimeSource;
    private Task? _runTask;
    private Task? _disconnectTask;
    private TaskCompletionSource _connectedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private CyberCommWebSocketPeer? _peer;
    private ClientWebSocket _clientWebSocket = new();
    private bool _manualDisconnect;

    public string? ServerUrl { get; set; }
    public bool IsConnected => State == CyberCommClientState.Connected;
    public bool AutoReconnect { get; set; } = true;
    public int ReconnectMaxTimes { get; set; } = -1;
    public int BufferLength { get; set; } = 16 * 1024;
    public int ReconnectTryDelay { get; set; } = 100;
    public bool AutoReceiveCompletedMessage { get; set; } = true;
    public CyberCommClientState State { get; private set; } = CyberCommClientState.Created;

    [Obsolete("请使用 MessageHandler")]
    public event EventHandler<CyberCommClientEventArgs>? MessageReceived;
    [Obsolete("请使用 ConnectionClosedHandler")]
    public event EventHandler? ConnectionClosed;
    [Obsolete("请使用 ConnectedHandler")]
    public event EventHandler? Connected;

    public Func<CyberCommClientEventArgs, CancellationToken, ValueTask>? MessageHandler { get; set; }
    public Func<CancellationToken, ValueTask>? ConnectedHandler { get; set; }
    public Func<CancellationToken, ValueTask>? ConnectionClosedHandler { get; set; }
    /// <summary>接收传输或用户回调异常；该回调自身的异常会被隔离。</summary>
    public Action<Exception>? ErrorHandler { get; set; }

    /// <summary>
    /// 当前 WebSocket。重连时该实例会被替换。
    /// </summary>
    public ClientWebSocket ClientWebSocket
    {
        get => _clientWebSocket;
        set => _clientWebSocket = value ?? throw new ArgumentNullException(nameof(value));
    }

    public CyberCommClient()
    {
    }

    public CyberCommClient(string serverUrl, bool autoReconnect = true, bool autoReceiveCompletedMessage = true)
    {
        ServerUrl = serverUrl;
        AutoReconnect = autoReconnect;
        AutoReceiveCompletedMessage = autoReceiveCompletedMessage;
    }

    public CyberCommClient(CyberCommClientOptions options)
    {
        _configuredOptions = options ?? throw new ArgumentNullException(nameof(options));
        ServerUrl = options.ServerUri.ToString();
        AutoReconnect = options.Reconnect.Enabled;
        ReconnectMaxTimes = options.Reconnect.MaxAttempts;
        ReconnectTryDelay = checked((int)Math.Min(int.MaxValue, options.Reconnect.InitialDelay.TotalMilliseconds));
        AutoReceiveCompletedMessage = options.AssembleWebSocketMessages;
    }

    /// <summary>
    /// 连接服务器。并发调用共享同一个连接过程。
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        Task? disconnectTask = null;
        Task connectionTask = Task.CompletedTask;
        await _lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (State == CyberCommClientState.Connected) return;
            if (State == CyberCommClientState.Disconnecting)
                disconnectTask = _disconnectTask ?? Task.CompletedTask;
            else if (State == CyberCommClientState.Faulted && _runTask is { IsCompleted: false })
                throw new InvalidOperationException("上一次连接循环仍未退出，无法创建新连接");
            else if (_runTask is null || _runTask.IsCompleted)
            {
                var options = GetEffectiveOptions();
                options.Validate();
                _manualDisconnect = false;
                _lifetimeSource?.Dispose();
                _lifetimeSource = new CancellationTokenSource();
                _connectedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
                State = CyberCommClientState.Connecting;
                _runTask = RunConnectionLoopAsync(options, _lifetimeSource.Token);
            }
            if (disconnectTask is null)
                connectionTask = _connectedSignal.Task;
        }
        finally
        {
            _lifecycleGate.Release();
        }
        if (disconnectTask is not null)
        {
            await disconnectTask.WaitAsync(cancellationToken).ConfigureAwait(false);
            await ConnectAsync(cancellationToken).ConfigureAwait(false);
            return;
        }
        await connectionTask.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 主动断开。主动断开不会触发自动重连。
    /// </summary>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        Task disconnectTask;
        await _lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (State is CyberCommClientState.Created or CyberCommClientState.Disconnected)
            {
                State = CyberCommClientState.Disconnected;
                return;
            }
            if (State == CyberCommClientState.Disconnecting && _disconnectTask is not null)
                disconnectTask = _disconnectTask;
            else
            {
                State = CyberCommClientState.Disconnecting;
                _manualDisconnect = true;
                disconnectTask = DisconnectCoreAsync(_peer, _runTask, GetEffectiveOptions().Limits.CloseTimeout);
                _disconnectTask = disconnectTask;
            }
        }
        finally
        {
            _lifecycleGate.Release();
        }
        await disconnectTask.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task DisconnectCoreAsync(CyberCommWebSocketPeer? peer, Task? runTask, TimeSpan closeTimeout)
    {
        var fullyStopped = true;
        using var timeout = new CancellationTokenSource();
        if (closeTimeout != Timeout.InfiniteTimeSpan) timeout.CancelAfter(closeTimeout);
        try
        {
            if (peer is null)
            {
                _lifetimeSource?.Cancel();
                _clientWebSocket.Abort();
            }
            else
            {
                var closeResult = await peer.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", timeout.Token).ConfigureAwait(false);
                if (!closeResult.IsSuccess)
                {
                    _lifetimeSource?.Cancel();
                    _clientWebSocket.Abort();
                }
            }
            if (runTask is not null)
                await runTask.WaitAsync(timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            _lifetimeSource?.Cancel();
            _clientWebSocket.Abort();
            if (runTask is not null)
            {
                try { await runTask.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false); }
                catch (TimeoutException) { fullyStopped = false; }
                catch (OperationCanceledException) { }
            }
        }
        finally
        {
            State = fullyStopped ? CyberCommClientState.Disconnected : CyberCommClientState.Faulted;
        }
    }

    public async ValueTask<CyberCommSendResult> SendTextAsync(string message, CancellationToken cancellationToken = default)
    {
        var peer = _peer;
        if (peer is null) return new(CyberCommSendStatus.Closed);
        return await peer.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<CyberCommSendResult> SendBinaryAsync(ReadOnlyMemory<byte> message, CancellationToken cancellationToken = default)
    {
        var peer = _peer;
        if (peer is null) return new(CyberCommSendStatus.Closed);
        return await peer.SendAsync(message, WebSocketMessageType.Binary, cancellationToken).ConfigureAwait(false);
    }

    [Obsolete("请使用 ConnectAsync")]
    public async Task StartCyberCommClient()
    {
        await ConnectAsync().ConfigureAwait(false);
        var runTask = _runTask;
        if (runTask is not null) await runTask.ConfigureAwait(false);
    }

    [Obsolete("请使用 SendTextAsync")]
    public async Task SendTextMessage(string message)
    {
        var result = await SendTextAsync(message).ConfigureAwait(false);
        if (!result.IsSuccess)
            throw new XFECyberCommException("客户端发送文本到服务器时出现异常", result.Exception ?? new InvalidOperationException(result.Status.ToString()));
    }

    [Obsolete("请使用 SendBinaryAsync")]
    public async Task SendBinaryMessage(byte[] message)
    {
        var result = await SendBinaryAsync(message).ConfigureAwait(false);
        if (!result.IsSuccess)
            throw new XFECyberCommException("客户端发送二进制数据到服务器时出现异常", result.Exception ?? new InvalidOperationException(result.Status.ToString()));
    }

    [Obsolete("请使用 DisconnectAsync")]
    public Task CloseCyberCommClient() => DisconnectAsync();

    private CyberCommClientOptions GetEffectiveOptions()
    {
        if (_configuredOptions is not null) return _configuredOptions;
        if (!Uri.TryCreate(ServerUrl, UriKind.Absolute, out var uri))
            throw new InvalidOperationException("未配置有效的 WebSocket 服务器地址");
        return new()
        {
            ServerUri = uri,
            AssembleWebSocketMessages = AutoReceiveCompletedMessage,
            Reconnect = new()
            {
                Enabled = AutoReconnect,
                MaxAttempts = ReconnectMaxTimes,
                InitialDelay = TimeSpan.FromMilliseconds(Math.Max(0, ReconnectTryDelay)),
                MaxDelay = TimeSpan.FromSeconds(30)
            }
        };
    }

    private async Task RunConnectionLoopAsync(CyberCommClientOptions options, CancellationToken cancellationToken)
    {
        var attempt = 0;
        var everConnected = false;
        Exception? lastConnectionError = null;
        try
        {
            while (!cancellationToken.IsCancellationRequested && !_manualDisconnect)
            {
                State = CyberCommClientState.Connecting;
                var socket = CreateClientWebSocket(options);
                _clientWebSocket = socket;
                try
                {
                    await socket.ConnectAsync(options.ServerUri, cancellationToken).ConfigureAwait(false);
                    attempt = 0;
                    lastConnectionError = null;
                    everConnected = true;
                    State = CyberCommClientState.Connected;
                    var peer = new CyberCommWebSocketPeer(socket, options.Limits);
                    _peer = peer;
                    _connectedSignal.TrySetResult();
                    await InvokeConnectedAsync(cancellationToken).ConfigureAwait(false);

                    try
                    {
                        await ReceiveLoopAsync(socket, peer, options, cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        _peer = null;
                        await peer.DisposeAsync().ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                }
                catch (Exception ex)
                {
                    lastConnectionError = ex;
                    await PublishErrorAsync(socket, new XFECyberCommException(everConnected ? "与服务器通讯期间发生异常" : "连接服务器时发生异常", ex), cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    socket.Dispose();
                }

                if (everConnected)
                {
                    everConnected = false;
                    State = CyberCommClientState.Disconnected;
                    await InvokeClosedAsync(CancellationToken.None).ConfigureAwait(false);
                    _connectedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
                }

                if (cancellationToken.IsCancellationRequested || _manualDisconnect)
                    break;
                if (!options.Reconnect.Enabled)
                {
                    if (lastConnectionError is null)
                    {
                        State = CyberCommClientState.Disconnected;
                        _connectedSignal.TrySetCanceled(new CancellationToken(true));
                    }
                    else
                    {
                        State = CyberCommClientState.Faulted;
                        _connectedSignal.TrySetException(new XFECyberCommException("连接服务器失败", lastConnectionError));
                    }
                    break;
                }
                if (options.Reconnect.MaxAttempts >= 0 && attempt >= options.Reconnect.MaxAttempts)
                {
                    State = CyberCommClientState.Faulted;
                    _connectedSignal.TrySetException(new XFECyberCommException("连接服务器失败，已达到最大重试次数"));
                    break;
                }

                attempt++;
                await Task.Delay(options.Reconnect.GetDelay(attempt), cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            if (State != CyberCommClientState.Faulted)
                State = CyberCommClientState.Disconnected;
            if (!_connectedSignal.Task.IsCompleted)
                _connectedSignal.TrySetCanceled(cancellationToken.IsCancellationRequested ? cancellationToken : new CancellationToken(true));
        }
    }

    private static ClientWebSocket CreateClientWebSocket(CyberCommClientOptions options)
    {
        var socket = new ClientWebSocket();
        socket.Options.KeepAliveInterval = options.KeepAliveInterval;
        if (options.RemoteCertificateValidationCallback is not null)
            socket.Options.RemoteCertificateValidationCallback = options.RemoteCertificateValidationCallback;
        foreach (var (name, value) in options.RequestHeaders)
            socket.Options.SetRequestHeader(name, value);
        return socket;
    }

    private async Task ReceiveLoopAsync(ClientWebSocket socket, CyberCommWebSocketPeer peer, CyberCommClientOptions options, CancellationToken cancellationToken)
    {
        var bufferSize = Math.Clamp(BufferLength, 1024, options.Limits.MaxWebSocketFrameBytes);
        var buffer = new byte[bufferSize];
        while (socket.State is WebSocketState.Open or WebSocketState.CloseSent)
        {
            using var receiveSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (options.Limits.ReceiveTimeout != Timeout.InfiniteTimeSpan)
                receiveSource.CancelAfter(options.Limits.ReceiveTimeout);
            var result = await socket.ReceiveAsync(buffer, receiveSource.Token).ConfigureAwait(false);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await peer.CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                    result.CloseStatusDescription ?? string.Empty, cancellationToken).ConfigureAwait(false);
                break;
            }

            if (!options.AssembleWebSocketMessages)
            {
                await PublishMessageAsync(socket, peer, result.MessageType, buffer.AsMemory(0, result.Count).ToArray(), result.EndOfMessage, cancellationToken).ConfigureAwait(false);
                continue;
            }

            using var message = new MemoryStream();
            var messageType = result.MessageType;
            await message.WriteAsync(buffer.AsMemory(0, result.Count), cancellationToken).ConfigureAwait(false);
            while (!result.EndOfMessage)
            {
                if (message.Length > options.Limits.MaxWebSocketMessageBytes)
                {
                    await peer.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Message is too large", cancellationToken).ConfigureAwait(false);
                    return;
                }
                result = await socket.ReceiveAsync(buffer, receiveSource.Token).ConfigureAwait(false);
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
            await PublishMessageAsync(socket, peer, messageType, message.ToArray(), true, cancellationToken, true).ConfigureAwait(false);
        }
    }

    private async ValueTask PublishMessageAsync(ClientWebSocket socket, CyberCommWebSocketPeer peer,
        WebSocketMessageType messageType, byte[] payload, bool endOfMessage, CancellationToken cancellationToken, bool validateUtf8 = false)
    {
        CyberCommClientEventArgs args;
        if (messageType == WebSocketMessageType.Text)
        {
            string text;
            try { text = validateUtf8 ? new UTF8Encoding(false, true).GetString(payload) : Encoding.UTF8.GetString(payload); }
            catch (DecoderFallbackException)
            {
                await peer.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid UTF-8", cancellationToken).ConfigureAwait(false);
                return;
            }
            args = new CyberCommClientEventArgsImpl(socket, text, endOfMessage);
        }
        else if (messageType == WebSocketMessageType.Binary)
            args = new CyberCommClientEventArgsImpl(socket, payload, endOfMessage);
        else
            throw new ArgumentOutOfRangeException(nameof(messageType));
        args = args.WithTransport(peer);
        if (MessageHandler is not null)
        {
            try { await MessageHandler(args, cancellationToken).ConfigureAwait(false); } catch (Exception ex) { InvokeErrorSafely(ex); }
        }
        InvokeSafely(MessageReceived, args);
    }

    private async ValueTask PublishErrorAsync(ClientWebSocket socket, XFECyberCommException exception, CancellationToken cancellationToken)
    {
        var args = new CyberCommClientEventArgsImpl(socket, exception);
        if (MessageHandler is not null)
        {
            try { await MessageHandler(args, cancellationToken).ConfigureAwait(false); } catch (Exception ex) { InvokeErrorSafely(ex); }
        }
        InvokeSafely(MessageReceived, args);
    }

    private async ValueTask InvokeConnectedAsync(CancellationToken cancellationToken)
    {
        if (ConnectedHandler is not null)
        {
            try { await ConnectedHandler(cancellationToken).ConfigureAwait(false); } catch (Exception ex) { InvokeErrorSafely(ex); }
        }
        InvokeSafely(Connected, EventArgs.Empty);
    }

    private async ValueTask InvokeClosedAsync(CancellationToken cancellationToken)
    {
        if (ConnectionClosedHandler is not null)
        {
            try { await ConnectionClosedHandler(cancellationToken).ConfigureAwait(false); } catch (Exception ex) { InvokeErrorSafely(ex); }
        }
        InvokeSafely(ConnectionClosed, EventArgs.Empty);
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

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        _lifetimeSource?.Dispose();
        _clientWebSocket.Dispose();
        _lifecycleGate.Dispose();
    }
}
