using System.Net.WebSockets;
using System.Threading.Channels;

namespace XFEExtension.NetCore.CyberComm;

/// <summary>
/// WebSocket 发送结果。
/// </summary>
public enum CyberCommSendStatus
{
    Success,
    Closed,
    QueueFull,
    Timeout,
    Cancelled,
    Failed
}

/// <summary>
/// WebSocket 发送结果。
/// </summary>
public readonly record struct CyberCommSendResult(CyberCommSendStatus Status, Exception? Exception = null)
{
    public bool IsSuccess => Status == CyberCommSendStatus.Success;
}

internal sealed class CyberCommWebSocketPeer : IAsyncDisposable
{
    private readonly WebSocket _webSocket;
    private readonly Channel<OutboundMessage> _outbound;
    private readonly TimeSpan _sendTimeout;
    private readonly CancellationTokenSource _stopSource = new();
    private readonly Task _sendLoop;

    public CyberCommWebSocketPeer(WebSocket webSocket, CyberCommLimitOptions limits)
    {
        _webSocket = webSocket;
        _sendTimeout = limits.SendTimeout;
        _outbound = Channel.CreateBounded<OutboundMessage>(new BoundedChannelOptions(limits.OutboundQueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
        _sendLoop = SendLoopAsync(_stopSource.Token);
    }

    public async ValueTask<CyberCommSendResult> SendAsync(
        ReadOnlyMemory<byte> payload,
        WebSocketMessageType messageType,
        CancellationToken cancellationToken)
    {
        if (_webSocket.State is not (WebSocketState.Open or WebSocketState.CloseReceived))
            return new(CyberCommSendStatus.Closed);

        var completion = new TaskCompletionSource<CyberCommSendResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var message = OutboundMessage.Data(payload.ToArray(), messageType, completion);
        using var timeout = CreateTimeoutSource(cancellationToken);
        try
        {
            await _outbound.Writer.WriteAsync(message, timeout.Token).ConfigureAwait(false);
            return await completion.Task.WaitAsync(timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new(CyberCommSendStatus.Cancelled);
        }
        catch (OperationCanceledException)
        {
            return new(CyberCommSendStatus.Timeout);
        }
        catch (ChannelClosedException)
        {
            return new(CyberCommSendStatus.Closed);
        }
    }

    public async ValueTask<CyberCommSendResult> CloseAsync(
        WebSocketCloseStatus closeStatus,
        string description,
        CancellationToken cancellationToken)
    {
        if (_webSocket.State is WebSocketState.Closed or WebSocketState.Aborted)
            return new(CyberCommSendStatus.Closed);

        var completion = new TaskCompletionSource<CyberCommSendResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var timeout = CreateTimeoutSource(cancellationToken);
        try
        {
            await _outbound.Writer.WriteAsync(OutboundMessage.Close(closeStatus, description, completion), timeout.Token).ConfigureAwait(false);
            return await completion.Task.WaitAsync(timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new(CyberCommSendStatus.Cancelled);
        }
        catch (OperationCanceledException)
        {
            return new(CyberCommSendStatus.Timeout);
        }
        catch (ChannelClosedException)
        {
            return new(CyberCommSendStatus.Closed);
        }
    }

    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        Exception? terminalException = null;
        try
        {
            await foreach (var message in _outbound.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                using var sendSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                if (_sendTimeout != Timeout.InfiniteTimeSpan) sendSource.CancelAfter(_sendTimeout);
                try
                {
                    if (message.IsClose)
                    {
                        if (_webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                            await _webSocket.CloseOutputAsync(message.CloseStatus, message.CloseDescription, sendSource.Token).ConfigureAwait(false);
                    }
                    else
                    {
                        await _webSocket.SendAsync(message.Payload, message.MessageType, true, sendSource.Token).ConfigureAwait(false);
                    }
                    message.Completion.TrySetResult(new(CyberCommSendStatus.Success));
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    message.Completion.TrySetResult(new(CyberCommSendStatus.Cancelled));
                    throw;
                }
                catch (OperationCanceledException)
                {
                    message.Completion.TrySetResult(new(CyberCommSendStatus.Timeout));
                    _webSocket.Abort();
                    break;
                }
                catch (Exception ex)
                {
                    terminalException = ex;
                    message.Completion.TrySetResult(new(CyberCommSendStatus.Failed, ex));
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            _outbound.Writer.TryComplete(terminalException);
            while (_outbound.Reader.TryRead(out var pending))
                pending.Completion.TrySetResult(new(terminalException is null ? CyberCommSendStatus.Closed : CyberCommSendStatus.Failed, terminalException));
        }
    }

    private CancellationTokenSource CreateTimeoutSource(CancellationToken cancellationToken)
    {
        var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stopSource.Token);
        if (_sendTimeout != Timeout.InfiniteTimeSpan)
            source.CancelAfter(_sendTimeout);
        return source;
    }

    public async ValueTask DisposeAsync()
    {
        _outbound.Writer.TryComplete();
        _stopSource.Cancel();
        try { await _sendLoop.ConfigureAwait(false); } catch (OperationCanceledException) { }
        _stopSource.Dispose();
    }

    private sealed record OutboundMessage(
        ReadOnlyMemory<byte> Payload,
        WebSocketMessageType MessageType,
        bool IsClose,
        WebSocketCloseStatus CloseStatus,
        string CloseDescription,
        TaskCompletionSource<CyberCommSendResult> Completion)
    {
        public static OutboundMessage Data(byte[] payload, WebSocketMessageType messageType, TaskCompletionSource<CyberCommSendResult> completion)
            => new(payload, messageType, false, WebSocketCloseStatus.Empty, string.Empty, completion);

        public static OutboundMessage Close(WebSocketCloseStatus closeStatus, string description, TaskCompletionSource<CyberCommSendResult> completion)
            => new(ReadOnlyMemory<byte>.Empty, WebSocketMessageType.Close, true, closeStatus, description, completion);
    }
}
