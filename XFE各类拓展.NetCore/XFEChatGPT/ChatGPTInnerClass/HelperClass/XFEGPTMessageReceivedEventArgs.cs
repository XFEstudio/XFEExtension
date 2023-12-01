namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass
{
    /// <summary>
    /// 从服务器接收到的GPT消息
    /// </summary>
    public abstract class XFEGPTMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 消息接收器ID
        /// </summary>
        public string messageId { get; set; }
        /// <summary>
        /// 生成状态
        /// </summary>
        public GenerateState generateState { get; set; }
        /// <summary>
        /// 从服务器接收到的GPT消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageId"></param>
        /// <param name="generateState"></param>
        public XFEGPTMessageReceivedEventArgs(string message, string messageId, GenerateState generateState)
        {
            this.message = message;
            this.messageId = messageId;
            this.generateState = generateState;
        }
    }
}