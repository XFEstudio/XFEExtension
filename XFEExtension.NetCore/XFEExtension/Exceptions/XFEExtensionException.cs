namespace XFEExtension.NetCore;

/// <summary>
/// XFEExtension的异常
/// </summary>
public class XFEExtensionException : Exception
{
    /// <summary>
    /// XFEExtension的异常
    /// </summary>
    /// <param name="message"></param>
    internal XFEExtensionException(string message) : base(message) { }
    /// <summary>
    /// XFEExtension的异常
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    internal XFEExtensionException(string message, Exception innerException) : base(message, innerException) { }
}
