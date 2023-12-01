namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass
{
    /// <summary>
    /// Token令牌的使用情况
    /// </summary>
    /// <remarks>
    /// Token令牌的使用情况
    /// </remarks>
    /// <param name="promptTokens"></param>
    /// <param name="completionTokens"></param>
    /// <param name="totalTokens"></param>
    public class TokenUsage(int promptTokens, int completionTokens, int totalTokens)
    {
        /// <summary>
        /// 用于提示的Token令牌
        /// </summary>
        public int PromptTokens { get; private set; } = promptTokens;
        /// <summary>
        /// 用于完成的Token令牌
        /// </summary>
        public int CompletionTokens { get; private set; } = completionTokens;
        /// <summary>
        /// 总共使用的Token令牌
        /// </summary>
        public int TotalTokens { get; private set; } = totalTokens;
    }
}