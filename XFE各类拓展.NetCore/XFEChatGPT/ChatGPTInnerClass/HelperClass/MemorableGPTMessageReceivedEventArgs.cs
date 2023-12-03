namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

/// <summary>
/// 从服务器接收到的GPT消息（记忆模式）
/// </summary>
/// <param name="message"></param>
/// <param name="id"></param>
/// <param name="generateState"></param>
/// <param name="dialogId"></param>
public abstract class MemorableGPTMessageReceivedEventArgs(string message, string id, GenerateState generateState, string dialogId) : XFEGPTMessageReceivedEventArgs(message, id, generateState)
{
    /// <summary>
    /// 对话ID
    /// </summary>
    public string DialogId { get; set; } = dialogId;
}