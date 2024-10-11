namespace XUnitConsole;

/// <summary>
/// Json节点
/// </summary>
public class JsonNode : INodeBase
{
    ///<inheritdoc/>
    public int Layer { get; set; }
    ///<inheritdoc/>
    public string PropertyName { get; set; } = string.Empty;
}
