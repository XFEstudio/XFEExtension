namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// Json属性节点
/// </summary>
[Obsolete("请使用 XFEJsonNode。旧节点树将在下一个主版本中删除。")]
public class JsonPropertyNode : JsonNode, IValueNode
{
    /// <summary>
    /// 获取或设置当前 JSON 属性值在转换时使用的值类型。
    /// </summary>
    public ValueType ValueType { get; set; } = ValueType.None;
    /// <summary>
    /// 获取或设置当前 JSON 属性节点保存的字符串值。
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
