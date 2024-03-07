using System.Net.WebSockets;
using System.Text;

namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// XCC网络通讯事件
/// </summary>
public abstract class XCCMessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// 触发事件的群组
    /// </summary>
    public XCCGroup Group { get; }
    /// <summary>
    /// WebSocket明文传输客户端
    /// </summary>
    public ClientWebSocket? TextMessageClientWebSocket { get; private set; }
    /// <summary>
    /// WebSocket文件传输客户端
    /// </summary>
    public ClientWebSocket? FileTransportClientWebSocket { get; private set; }
    /// <summary>
    /// XCC服务器连接类型
    /// </summary>
    public XCCClientType XCCClientType { get; }
    /// <summary>
    /// 消息ID
    /// </summary>
    public string? MessageId { get; }
    /// <summary>
    /// 群组ID
    /// </summary>
    public string GroupId
    {
        get
        {
            return Group.GroupId;
        }
    }
    /// <summary>
    /// 发送者
    /// </summary>
    public string? Sender { get; }
    /// <summary>
    /// 回复文本消息
    /// </summary>
    /// <param name="message">待发送的文本</param>
    /// <returns>发送进程</returns>
    public async Task ReplyTextMessage(string message)
    {
        try
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
            await TextMessageClientWebSocket!.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("收到服务器端数据后客户端回复文本数据时出现异常", ex);
        }
    }
    /// <summary>
    /// 回复二进制消息
    /// </summary>
    /// <param name="message">二进制消息</param>
    /// <exception cref="XFECyberCommException"></exception>
    public async Task ReplyBinaryMessage(byte[] message)
    {
        try
        {
            await FileTransportClientWebSocket!.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Binary, true, CancellationToken.None);
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
            if (TextMessageClientWebSocket?.State == WebSocketState.Open)
                await TextMessageClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端主动关闭连接", CancellationToken.None);
            if (FileTransportClientWebSocket?.State == WebSocketState.Open)
                await FileTransportClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端主动关闭连接", CancellationToken.None);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("客户端关闭连接时出现异常", ex);
        }
    }
    internal XCCMessageReceivedEventArgs(XCCGroup group, ClientWebSocket? textMessageClientWebSocket, ClientWebSocket? fileTransportClientWebSocket, XCCClientType xCCClientType, string? sender, string? messageId)
    {
        Group = group;
        TextMessageClientWebSocket = textMessageClientWebSocket;
        FileTransportClientWebSocket = fileTransportClientWebSocket;
        XCCClientType = xCCClientType;
        Sender = sender;
        MessageId = messageId;
    }
}
