using System.Net.WebSockets;

namespace XFE各类拓展.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// XCC网络通讯接收到明文消息事件
/// </summary>
public abstract class XCCTextMessageReceivedEventArgs : XCCMessageReceivedEventArgs
{
    /// <summary>
    /// 返回文本消息类型
    /// </summary>
    public XCCTextMessageType MessageType { get; }
    /// <summary>
    /// 文本消息
    /// </summary>
    public string TextMessage { get; }
    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SendTime { get; }
    /// <summary>
    /// 是否为历史消息
    /// </summary>
    public bool IsHistory { get; }
    internal XCCTextMessageReceivedEventArgs(XCCGroup group, ClientWebSocket? textMessageClientWebSocket, ClientWebSocket? fileTransportClientWebSocket, XCCClientType xCCClientType, string messageId, XCCTextMessageType messageType, string message, string sender, DateTime sendTime, bool isHistory) : base(group, textMessageClientWebSocket, fileTransportClientWebSocket, xCCClientType, sender, messageId)
    {
        MessageType = messageType;
        TextMessage = message;
        SendTime = sendTime;
        IsHistory = isHistory;
    }

}
