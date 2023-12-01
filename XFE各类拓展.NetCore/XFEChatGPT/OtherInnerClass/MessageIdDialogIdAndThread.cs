namespace XFE各类拓展.NetCore.XFEChatGPT.OtherInnerClass
{
    class MessageIdDialogIdAndThread : MessageIdAndThread
    {
        public string dialogId { get; set; }
        public MessageIdDialogIdAndThread(string messageId, string dialogId, Thread thread) : base(messageId, thread)
        {
            this.dialogId = dialogId;
        }
    }
}