namespace XFE各类拓展.NetCore;

/// <summary>
/// XFE邮件的异常
/// </summary>
public class XFEMailException : XFEExtensionException
{
    internal XFEMailException(string message) : base(message) { }
    internal XFEMailException(string message, Exception innerException) : base(message, innerException) { }
}
