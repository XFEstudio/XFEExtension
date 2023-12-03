namespace XFE各类拓展.NetCore.XFEChatGPT.OtherInnerClass;

class MessageIdAndThread(string messageId, Thread thread)
{
    public string MessageId { get; set; } = messageId;
    public Thread Thread { get; set; } = thread;
}