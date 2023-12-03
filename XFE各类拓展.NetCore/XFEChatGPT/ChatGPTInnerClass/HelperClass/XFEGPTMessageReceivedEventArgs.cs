namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

/// <summary>
/// 从服务器接收到的GPT消息
/// </summary>
/// <remarks>
/// 从服务器接收到的GPT消息
/// </remarks>
/// <param name="message"></param>
/// <param name="messageId"></param>
/// <param name="generateState"></param>
public abstract class XFEGPTMessageReceivedEventArgs(string message, string messageId, GenerateState generateState) : EventArgs
{
    /// <summary>
    /// 消息内容
    /// </summary>
    public string Message { get; set; } = message;
    /// <summary>
    /// 消息接收器ID
    /// </summary>
    public string MessageId { get; set; } = messageId;
    /// <summary>
    /// 生成状态
    /// </summary>
    public GenerateState GenerateState { get; set; } = generateState;
}