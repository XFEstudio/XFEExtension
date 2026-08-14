namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// 值类型
/// </summary>
[Obsolete("请使用 XFEJsonValueKind 或强类型读取 API。")]
public enum ValueType
{
    /// <summary>
    /// 无类型
    /// </summary>
    None,
    /// <summary>
    /// string文本类型
    /// </summary>
    Text,
    /// <summary>
    /// char类型
    /// </summary>
    Char,
    /// <summary>
    /// bool类型
    /// </summary>
    Boolean,
    /// <summary>
    /// int类型
    /// </summary>
    Int,
    /// <summary>
    /// long类型
    /// </summary>
    Long,
    /// <summary>
    /// 浮点类型
    /// </summary>
    Float,
    /// <summary>
    /// 空类型
    /// </summary>
    Null
}
