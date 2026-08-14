namespace XFEExtension.NetCore.XFETransform.JsonConverter;

[Obsolete("请使用 XFEJsonNode.SelectProperties 或 ProjectArray。")]
class NodeOperator
{
    /// <summary>
    /// 从列表型复杂节点的每个子对象中提取指定属性，并打包为对象字典列表。
    /// </summary>
    /// <param name="nodeProperties">需要从每个子对象中提取的属性名称。</param>
    /// <param name="jsonComplexPropertyNode">包含待处理子对象的复杂 JSON 节点。</param>
    /// <returns>保留原始节点并携带已打包对象字典列表的可查询节点。</returns>
    public static QueryableJsonNode PackageInList(string[] nodeProperties, JsonComplexPropertyNode jsonComplexPropertyNode)
    {
        var valueNodes = new List<Dictionary<string, ValueNode>>();
        var selectableJsonNode = new QueryableJsonNode(jsonComplexPropertyNode)
        {
            InnerValue = valueNodes
        };
        foreach (var node in jsonComplexPropertyNode.DescendingNodes)
        {
            if (node is not JsonComplexPropertyNode complexPropertyNode)
                continue;

            valueNodes.Add([]);
            foreach (var childNode in complexPropertyNode.DescendingNodes)
                if (childNode is JsonPropertyNode jsonPropertyNode && nodeProperties.Contains(jsonPropertyNode.PropertyName))
                    valueNodes[^1].Add(jsonPropertyNode.PropertyName, new(jsonPropertyNode.Value, jsonPropertyNode.ValueType));
        }
        return selectableJsonNode;
    }

    /// <summary>
    /// 从复杂节点中提取指定的直接属性，并打包为单个属性字典。
    /// </summary>
    /// <param name="nodeProperties">需要从复杂节点中提取的属性名称。</param>
    /// <param name="jsonComplexPropertyNode">包含待处理属性的复杂 JSON 节点。</param>
    /// <returns>保留原始节点并携带已打包属性字典的可查询节点。</returns>
    public static QueryableJsonNode PackageToList(string[] nodeProperties, JsonComplexPropertyNode jsonComplexPropertyNode)
    {
        var valueNodes = new Dictionary<string, ValueNode>();
        var selectableJsonNode = new QueryableJsonNode(jsonComplexPropertyNode)
        {
            InnerValue = valueNodes
        };
        foreach (var node in jsonComplexPropertyNode.DescendingNodes)
        {
            if (node is JsonPropertyNode jsonPropertyNode && nodeProperties.Contains(jsonPropertyNode.PropertyName))
            {
                valueNodes.Add(jsonPropertyNode.PropertyName, new(jsonPropertyNode.Value, jsonPropertyNode.ValueType));
            }
        }
        return selectableJsonNode;
    }
}
