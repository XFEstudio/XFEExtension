using XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;

namespace XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

class PrivateMessageChoice : MessageChoice
{
    /// <summary>
    /// 创建同时包含流式增量消息和完整消息的 GPT 候选结果。
    /// </summary>
    /// <param name="delta">流式接收时的增量消息。</param>
    /// <param name="message">本次候选结果的完整消息。</param>
    /// <param name="finishReason">消息生成结束的原因。</param>
    /// <param name="index">候选结果在响应中的索引。</param>
    public PrivateMessageChoice(GPTMessage delta, GPTMessage message, string finishReason, int index) : base(delta, message, finishReason, index) { }
    /// <summary>
    /// 创建仅包含完整消息的 GPT 候选结果。
    /// </summary>
    /// <param name="message">本次候选结果的完整消息。</param>
    /// <param name="finishReason">消息生成结束的原因。</param>
    /// <param name="index">候选结果在响应中的索引。</param>
    public PrivateMessageChoice(GPTMessage message, string finishReason, int index) : base(message, finishReason, index) { }
}
