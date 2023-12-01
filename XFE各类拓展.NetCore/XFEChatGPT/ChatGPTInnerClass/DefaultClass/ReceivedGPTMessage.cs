namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass
{
    /// <summary>
    /// 接收到的GPT消息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="object"></param>
    /// <param name="created"></param>
    /// <param name="model"></param>
    /// <param name="usage"></param>
    /// <param name="choices"></param>
    public abstract class ReceivedGPTMessage(string id, string @object, long created, string model, TokenUsage usage, MessageChoice[] choices)
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string Id { get; private set; } = id;
        /// <summary>
        /// 消息对象
        /// </summary>
        public string Object { get; private set; } = @object;
        /// <summary>
        /// 消息创建时间
        /// </summary>
        public long Created { get; private set; } = created;
        /// <summary>
        /// 使用的GPT模型
        /// </summary>
        public string Model { get; private set; } = model;
        /// <summary>
        /// Token令牌的使用情况
        /// </summary>
        public TokenUsage Usage { get; private set; } = usage;
        /// <summary>
        /// GPT返回的具体数据（数组的形式）
        /// </summary>
        public MessageChoice[] Choices { get; private set; } = choices;
    }
}