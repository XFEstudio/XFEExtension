using XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;

namespace XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

/// <summary>
/// XFEGPT消息（具有消息ID）
/// </summary>
/// <remarks>
/// XFEGPT消息（具有消息ID）
/// </remarks>
/// <param name="messageId">消息ID</param>
/// <param name="gPTMessage">GPT消息</param>
public class XFEGPTMessage(string messageId, GPTMessage gPTMessage)
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; set; } = messageId;
    /// <summary>
    /// GPT消息
    /// </summary>
    public GPTMessage GPTMessage { get; set; } = gPTMessage;
}