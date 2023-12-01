namespace XFE各类拓展.NetCore.XFEChatGPT.OtherInnerClass
{
    class MessageIdAndThread
    {
        public string messageId { get; set; }
        public Thread thread { get; set; }
        public MessageIdAndThread(string messageId, Thread thread)
        {
            this.messageId = messageId;
            this.thread = thread;
        }
    }
}