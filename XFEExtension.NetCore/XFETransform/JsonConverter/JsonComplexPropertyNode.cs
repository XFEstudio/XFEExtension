namespace XUnitConsole;

/// <summary>
/// Json复杂节点
/// </summary>
public class JsonComplexPropertyNode : JsonNode, IQueryableJsonNode
{
    ///<inheritdoc/>
    public QueryableJsonNode this[params string[] nodeProperties] => JsonNodeConverter.AnalyzePropertyArray(nodeProperties, this);
    /// <summary>
    /// 是否是列表
    /// </summary>
    public bool IsList { get; set; }
    /// <summary>
    /// 子节点
    /// </summary>
    public List<JsonNode> DescendingNodes { get; set; } = [];
}