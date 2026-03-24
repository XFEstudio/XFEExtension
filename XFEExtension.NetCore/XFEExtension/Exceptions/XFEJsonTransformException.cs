namespace XFEExtension.NetCore;

/// <summary>
/// XFEJson转换的异常
/// </summary>
public class XFEJsonTransformException : XFEExtensionException
{
    /// <summary>
    /// XFEJson转换的异常
    /// </summary>
    /// <param name="message"></param>
    internal XFEJsonTransformException(string message) : base(message) { }
    /// <summary>
    /// XFEJson转换的异常
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    internal XFEJsonTransformException(string message, Exception innerException) : base(message, innerException) { }
}
