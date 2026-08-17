using System.Collections.Specialized;
using System.Net.WebSockets;
using System.Text;
using XFEExtension.NetCore.Exceptions;

namespace XFEExtension.NetCore.CyberComm;

/// <summary>
/// CyberComm服务器事件参数
/// </summary>
/// <param name="RequestUrl">请求时的URL地址</param>
/// <param name="MessageType"> 消息类型 </param>
/// <param name="CurrentWebSocket"> 当前WebSocket </param>
/// <param name="WSHeader"> 客户端请求头 </param>
/// <param name="Exception"> 异常消息（如果有的话） </param>
/// <param name="IPAddress"> 客户端IP地址 </param>
/// <param name="TextMessage"> 文本消息 </param>
/// <param name="BinaryMessage"> 二进制消息 </param>
/// <param name="EndOfMessage">消息是否结束</param>
public abstract record CyberCommServerEventArgs(Uri? RequestUrl, BackMessageType MessageType, WebSocket CurrentWebSocket, NameValueCollection WSHeader, XFECyberCommException? Exception, string IPAddress, string? TextMessage, byte[]? BinaryMessage, bool EndOfMessage)
{
    private Func<ReadOnlyMemory<byte>, WebSocketMessageType, CancellationToken, ValueTask<CyberCommSendResult>>? SendHandler { get; init; }
    private Func<WebSocketCloseStatus, string, CancellationToken, ValueTask<CyberCommSendResult>>? CloseHandler { get; init; }

    /// <summary>
    /// 发送文本消息
    /// </summary>
    /// <param name="message">待发送的文本</param>
    /// <exception cref="XFECyberCommException"></exception>
    /// <returns>发送进程</returns>
    public async Task ReplyMessage(string message)
    {
        try
        {
            var sendBuffer = Encoding.UTF8.GetBytes(message);
            if (SendHandler is not null)
            {
                var result = await SendHandler(sendBuffer, WebSocketMessageType.Text, CancellationToken.None).ConfigureAwait(false);
                if (!result.IsSuccess) throw result.Exception ?? new InvalidOperationException($"发送失败：{result.Status}");
            }
            else
                await CurrentWebSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("收到客户端数据后服务器端回复文本时出现异常", ex);
        }
    }
    /// <summary>
    /// 发送二进制消息
    /// </summary>
    /// <param name="bytes">二进制消息</param>
    /// <exception cref="XFECyberCommException"></exception>
    /// <returns></returns>
    public async Task ReplyBinaryMessage(byte[] bytes)
    {
        try
        {
            if (SendHandler is not null)
            {
                var result = await SendHandler(bytes, WebSocketMessageType.Binary, CancellationToken.None).ConfigureAwait(false);
                if (!result.IsSuccess) throw result.Exception ?? new InvalidOperationException($"发送失败：{result.Status}");
            }
            else
                await CurrentWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("收到客户端数据后服务器端回复二进制数据时出现异常", ex);
        }
    }
    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <exception cref="XFECyberCommException"></exception>
    public async Task Close()
    {
        try
        {
            if (CloseHandler is not null)
                await CloseHandler(WebSocketCloseStatus.NormalClosure, "Connection Closed", CancellationToken.None).ConfigureAwait(false);
            else
                await CurrentWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection Closed", CancellationToken.None);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("关闭服务器端连接时出现异常", ex);
        }
    }
    /// <summary>
    /// 强制关闭连接
    /// </summary>
    /// <exception cref="XFECyberCommException"></exception>
    public void ForceClose()
    {
        try
        {
            CurrentWebSocket.Abort();
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("强制关闭服务器端连接时出现异常", ex);
        }
    }
    internal CyberCommServerEventArgs(Uri? requestUrl, WebSocket webSocket, string message, string ipAddress, NameValueCollection wsHeader, bool endOfMessage) : this(requestUrl, BackMessageType.Text, webSocket, wsHeader, null, ipAddress, message, null, endOfMessage)
    {
    }
    internal CyberCommServerEventArgs(Uri? requestUrl, WebSocket webSocket, byte[] bytes, string ipAddress, NameValueCollection wsHeader, bool endOfMessage) : this(requestUrl, BackMessageType.Binary, webSocket, wsHeader, null, ipAddress, null, bytes, endOfMessage)
    {
    }
    internal CyberCommServerEventArgs(Uri? requestUrl, WebSocket webSocket, XFECyberCommException ex, string ipAddress, NameValueCollection wsHeader) : this(requestUrl, BackMessageType.Error, webSocket, wsHeader, ex, ipAddress, null, null, true)
    {
    }

    internal CyberCommServerEventArgs WithTransport(CyberCommWebSocketPeer peer) => this with
    {
        SendHandler = peer.SendAsync,
        CloseHandler = peer.CloseAsync
    };
}
