using System.Net.WebSockets;

namespace XFE各类拓展.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// XCC网络通讯接收到二进制消息事件
/// </summary>
public abstract class XCCBinaryMessageReceivedEventArgs : XCCMessageReceivedEventArgs
{
    /// <summary>
    /// 返回二进制消息类型
    /// </summary>
    public XCCBinaryMessageType MessageType { get; }
    /// <summary>
    /// 消息签名
    /// </summary>
    public string Signature { get; }
    /// <summary>
    /// 二进制消息
    /// </summary>
    public byte[] BinaryMessage { get; }
    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SendTime { get; }
    /// <summary>
    /// 是否为历史消息
    /// </summary>
    public bool IsHistory { get; }
    internal XCCBinaryMessageReceivedEventArgs(XCCGroup group, ClientWebSocket? textMessageClientWebSocket, ClientWebSocket? fileTransportClientWebSocket, XCCClientType xCCClientType, string sender, string messageId, byte[] buffer, XCCBinaryMessageType messageType, string signature, DateTime sendTime, bool isHistory) : base(group, textMessageClientWebSocket, fileTransportClientWebSocket, xCCClientType, sender, messageId)
    {
        BinaryMessage = buffer;
        MessageType = messageType;
        Signature = signature;
        SendTime = sendTime;
        IsHistory = isHistory;
    }
}
