namespace XFEExtension.NetCore.XFEChatGPT.OtherInnerClass;

class MessageIdAndThread(string messageId, Thread thread)
{
    public string MessageId { get; set; } = messageId;
    public Thread Thread { get; set; } = thread;
}