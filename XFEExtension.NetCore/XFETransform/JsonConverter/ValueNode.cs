namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// 值节点
/// </summary>
/// <param name="value">值</param>
/// <param name="valueType">值类型</param>
[Obsolete("请使用 XFEJsonNode。旧值节点将在下一个主版本中删除。")]
public class ValueNode(string value, ValueType valueType) : IValueNode
{
    /// <summary>
    /// 获取或设置该节点保存的字符串值。
    /// </summary>
    public string Value { get; set; } = value;
    /// <summary>
    /// 获取或设置该节点值在转换时使用的值类型。
    /// </summary>
    public ValueType ValueType { get; set; } = valueType;
    /// <summary>
    /// 字符串值
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Value;
}
