using XFEExtension.NetCore.ArrayExtension;

namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// XCC消息
/// </summary>
/// <remarks>
/// XCC消息
/// </remarks>
/// <param name="messageId">消息ID</param>
/// <param name="messageType">消息类型</param>
/// <param name="message">消息内容</param>
/// <param name="sender">发送者</param>
/// <param name="sendTime">发送时间</param>
/// <param name="groupId">群组ID</param>
public class XCCMessage(string messageId, XCCTextMessageType messageType, string message, string sender, DateTime sendTime, string groupId)
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; } = messageId;
    /// <summary>
    /// 消息类型
    /// </summary>
    public XCCTextMessageType MessageType { get; } = messageType;
    /// <summary>
    /// 消息内容
    /// </summary>
    public string Message { get; } = message;
    /// <summary>
    /// 发送者
    /// </summary>
    public string Sender { get; } = sender;
    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SendTime { get; } = sendTime;
    /// <summary>
    /// 群组ID
    /// </summary>
    public string GroupId { get; } = groupId;
    /// <summary>
    /// 封装为字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return new string[] { MessageId, MessageType.ToString(), Message, Sender, SendTime.ToString() }.ToXFEString();
    }
    /// <summary>
    /// 将封装后的XCC消息字符串转换为XCC消息对象
    /// </summary>
    /// <param name="xCCMessageStringFormat"></param>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public static XCCMessage ConvertToXCCMessage(string xCCMessageStringFormat, string groupId)
    {
        var unPackedMessage = xCCMessageStringFormat.ToXFEArray<string>();
        return new XCCMessage(unPackedMessage[0], (XCCTextMessageType)Enum.Parse(typeof(XCCTextMessageType), unPackedMessage[1]), unPackedMessage[2], unPackedMessage[3], DateTime.Parse(unPackedMessage[4]), groupId);
    }
}
