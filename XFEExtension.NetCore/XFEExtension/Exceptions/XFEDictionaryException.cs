namespace XFEExtension.NetCore;

/// <summary>
/// XFE字典的异常
/// </summary>
public class XFEDictionaryException : XFEExtensionException
{
    internal XFEDictionaryException(string message) : base(message) { }
    internal XFEDictionaryException(string message, Exception innerException) : base(message, innerException) { }
}
