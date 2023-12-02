namespace XFE各类拓展.NetCore.XFEChatGPT.OtherInnerClass
{
    class MessageIdDialogIdAndThread(string messageId, string dialogId, Thread thread) : MessageIdAndThread(messageId, thread)
    {
        public string DialogId { get; set; } = dialogId;
    }
}