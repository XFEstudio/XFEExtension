namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass
{
    /// <summary>
    /// GPT消息链的组成单元
    /// </summary>
    public abstract class MessageChoice
    {
        /// <summary>
        /// Delta消息（用于流式接收）
        /// </summary>
        public GPTMessage? Delta { get; private set; }
        /// <summary>
        /// 单个消息（用于单个接收）
        /// </summary>
        public GPTMessage Message { get; private set; }
        /// <summary>
        /// 结束原因
        /// </summary>
        public string FinishReason { get; private set; }
        /// <summary>
        /// 消息索引
        /// </summary>
        public int Index { get; private set; }
        /// <summary>
        /// GPT消息链的组成单元
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="message"></param>
        /// <param name="finishReason"></param>
        /// <param name="index"></param>
        public MessageChoice(GPTMessage delta, GPTMessage message, string finishReason, int index)
        {
            this.Delta = delta;
            this.Message = message;
            this.FinishReason = finishReason;
            this.Index = index;
        }
        /// <summary>
        /// GPT消息链的组成单元
        /// </summary>
        /// <param name="message">GPTMessage消息对象</param>
        /// <param name="finishReason">结束原因</param>
        /// <param name="index">消息索引</param>
        public MessageChoice(GPTMessage message, string finishReason, int index)
        {
            this.Message = message;
            this.FinishReason = finishReason;
            this.Index = index;
        }
    }
}