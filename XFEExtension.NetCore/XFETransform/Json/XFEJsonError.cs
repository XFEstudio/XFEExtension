using XFEExtension.NetCore.Exceptions;

namespace XFEExtension.NetCore.XFETransform.Json;

/// <summary>
/// JSON 语法或转换错误的详细信息。
/// </summary>
public sealed record XFEJsonError
{
    /// <summary>
    /// 错误消息。
    /// </summary>
    public required string Message { get; init; }
    /// <summary>
    /// JSON 路径。
    /// </summary>
    public required string Path { get; init; }
    /// <summary>
    /// 错误字符在源文本中的零基偏移量；非文本错误为 -1。
    /// </summary>
    public required int Position { get; init; }
    /// <summary>
    /// 一基行号；非文本错误为 0。
    /// </summary>
    public required int LineNumber { get; init; }
    /// <summary>
    /// 一基列号；非文本错误为 0。
    /// </summary>
    public required int ColumnNumber { get; init; }
}

/// <summary>
/// XFE JSON 语法、序列化或反序列化异常。
/// </summary>
public class XFEJsonException : XFEExtensionException
{
    /// <summary>
    /// 错误详情。
    /// </summary>
    public XFEJsonError Error { get; }
    /// <summary>
    /// JSON 路径。
    /// </summary>
    public string Path => Error.Path;
    /// <summary>
    /// 错误字符偏移量。
    /// </summary>
    public int Position => Error.Position;
    /// <summary>
    /// 错误行号。
    /// </summary>
    public int LineNumber => Error.LineNumber;
    /// <summary>
    /// 错误列号。
    /// </summary>
    public int ColumnNumber => Error.ColumnNumber;

    internal XFEJsonException(XFEJsonError error)
        : base(FormatMessage(error))
    {
        Error = error;
    }

    internal XFEJsonException(XFEJsonError error, Exception innerException)
        : base(FormatMessage(error), innerException)
    {
        Error = error;
    }

    private static string FormatMessage(XFEJsonError error)
    {
        var location = error.Position < 0
            ? string.Empty
            : $"（位置 {error.Position}，行 {error.LineNumber}，列 {error.ColumnNumber}）";
        return $"{error.Message} 路径：{error.Path}{location}";
    }
}

/// <summary>
/// JSON 值类型。
/// </summary>
public enum XFEJsonValueKind
{
    /// <summary>
    /// 尚未确定。
    /// </summary>
    Undefined,
    /// <summary>
    /// JSON 对象。
    /// </summary>
    Object,
    /// <summary>
    /// JSON 数组。
    /// </summary>
    Array,
    /// <summary>
    /// JSON 字符串。
    /// </summary>
    String,
    /// <summary>
    /// JSON 数字。
    /// </summary>
    Number,
    /// <summary>
    /// true。
    /// </summary>
    True,
    /// <summary>
    /// false。
    /// </summary>
    False,
    /// <summary>
    /// null。
    /// </summary>
    Null
}
