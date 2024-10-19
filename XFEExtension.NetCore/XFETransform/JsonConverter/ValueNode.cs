namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// 值节点
/// </summary>
/// <param name="value">值</param>
/// <param name="valueType">值类型</param>
public class ValueNode(string value, ValueType valueType) : IValueNode
{
    ///<inheritdoc/>
    public string Value { get; set; } = value;
    ///<inheritdoc/>
    public ValueType ValueType { get; set; } = valueType;
    /// <summary>
    /// 字符串值
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Value;
}
