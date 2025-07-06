using System.Net.WebSockets;
using System.Text;

namespace XFEExtension.NetCore.CyberComm;

/// <summary>
/// CyberComm客户端事件参数
/// </summary>
/// <param name="MessageType"> 消息类型 </param>
/// <param name="CurrentWebSocket"> 当前WebSocket </param>
/// <param name="TextMessage"> 文本消息 </param>
/// <param name="Exception"> 异常消息（如果有的话） </param>
/// <param name="BinaryMessage"> 二进制消息 </param>
/// <param name="EndOfMessage">是否发送完成</param>
public abstract record CyberCommClientEventArgs(BackMessageType MessageType, ClientWebSocket CurrentWebSocket, string? TextMessage, XFECyberCommException? Exception, byte[]? BinaryMessage, bool EndOfMessage)
{
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
            byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
            await CurrentWebSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("收到服务器端数据后客户端回复文本数据时出现异常", ex);
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
            await CurrentWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection Closed", CancellationToken.None);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("关闭客户端连接时出现异常", ex);
        }
    }
    internal CyberCommClientEventArgs(ClientWebSocket clientWebSocket, string message, bool endOfMessage) : this(BackMessageType.Text, clientWebSocket, message, null, null, endOfMessage)
    {
    }
    internal CyberCommClientEventArgs(ClientWebSocket clientWebSocket, byte[] bytes, bool endOfMessage) : this(BackMessageType.Binary, clientWebSocket, null, null, bytes, endOfMessage)
    {
    }
    internal CyberCommClientEventArgs(ClientWebSocket clientWebSocket, XFECyberCommException ex) : this(BackMessageType.Error, clientWebSocket, null, ex, null, true)
    {
    }
}