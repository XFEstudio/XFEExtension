namespace XFE各类拓展.NetCore;

/// <summary>
/// XFE各类拓展的异常
/// </summary>
public class XFEExtensionException : Exception
{
    /// <summary>
    /// XFE各类拓展的异常
    /// </summary>
    /// <param name="message"></param>
    internal XFEExtensionException(string message) : base(message) { }
    /// <summary>
    /// XFE各类拓展的异常
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    internal XFEExtensionException(string message, Exception innerException) : base(message, innerException) { }
}
