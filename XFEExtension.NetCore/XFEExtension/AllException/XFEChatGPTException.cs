namespace XFEExtension.NetCore;

/// <summary>
/// XFEChatGPT的异常
/// </summary>
public class XFEChatGPTException : XFEExtensionException
{
    /// <summary>
    /// XFEChatGPT的异常
    /// </summary>
    /// <param name="message"></param>
    internal XFEChatGPTException(string message) : base(message) { }
    internal XFEChatGPTException(string message, Exception innerException) : base(message, innerException) { }
}
