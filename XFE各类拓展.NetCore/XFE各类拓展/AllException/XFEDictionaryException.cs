namespace XFE各类拓展.NetCore;

/// <summary>
/// XFE字典的异常
/// </summary>
public class XFEDictionaryException : XFEExtensionException
{
    internal XFEDictionaryException(string message) : base(message) { }
    internal XFEDictionaryException(string message, Exception innerException) : base(message, innerException) { }
}
