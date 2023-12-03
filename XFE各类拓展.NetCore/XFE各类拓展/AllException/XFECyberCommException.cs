namespace XFE各类拓展.NetCore;

/// <summary>
/// XFE网络通信的异常
/// </summary>
public class XFECyberCommException : XFEExtensionException
{
    /// <summary>
    /// XFE网络通信的异常
    /// </summary>
    /// <param name="message"></param>
    internal XFECyberCommException(string message) : base(message) { }
    internal XFECyberCommException(string message, Exception innerException) : base(message, innerException) { }
}
