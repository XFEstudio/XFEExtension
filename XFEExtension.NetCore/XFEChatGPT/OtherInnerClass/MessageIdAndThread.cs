namespace XFEExtension.NetCore.XFEChatGPT.OtherInnerClass;

class MessageIdAndThread(string messageId, Thread thread)
{
    /// <summary>
    /// 获取或设置后台处理线程所对应的消息 ID。
    /// </summary>
    public string MessageId { get; set; } = messageId;
    /// <summary>
    /// 获取或设置处理该消息的线程。
    /// </summary>
    public Thread Thread { get; set; } = thread;
}
