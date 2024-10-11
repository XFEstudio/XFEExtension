namespace XUnitConsole;

/// <summary>
/// Json属性节点
/// </summary>
public class JsonPropertyNode : JsonNode, IValueNode
{
    ///<inheritdoc/>
    public ValueType ValueType { get; set; } = ValueType.None;
    ///<inheritdoc/>
    public string Value { get; set; } = string.Empty;
}
