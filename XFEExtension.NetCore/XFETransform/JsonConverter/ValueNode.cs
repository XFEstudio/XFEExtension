namespace XUnitConsole;

public class ValueNode(string value, ValueType valueType) : IValueNode
{
    public string Value { get; set; } = value;
    public ValueType ValueType { get; set; } = valueType;
}
