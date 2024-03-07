namespace XFEExtension.NetCore.XFEChatGPT;

/// <summary>
/// XFE通讯协议
/// </summary>
public enum XFEComProtocol
{
    /// <summary>
    /// 响应速度快速，不安全的通讯协议
    /// </summary>
    XFEFAST,
    /// <summary>
    /// 响应速度适中，安全的通讯协议
    /// </summary>
    XFEHARD
}