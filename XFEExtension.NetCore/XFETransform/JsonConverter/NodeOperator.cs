namespace XFEExtension.NetCore.XFETransform.JsonConverter;

class NodeOperator
{
    public static QueryableJsonNode PackageInList(string[] nodeProperties, JsonComplexPropertyNode jsonComplexPropertyNode)
    {
        var valueNodes = new List<Dictionary<string, ValueNode>>();
        var selectableJsonNode = new QueryableJsonNode(jsonComplexPropertyNode)
        {
            InnerValue = valueNodes
        };
        foreach (var node in jsonComplexPropertyNode.DescendingNodes)
        {
            if (node is JsonComplexPropertyNode complexPropertyNode)
            {
                valueNodes.Add([]);
                foreach (var childNode in complexPropertyNode.DescendingNodes)
                {
                    if (childNode is JsonPropertyNode jsonPropertyNode && nodeProperties.Contains(jsonPropertyNode.PropertyName))
                    {
                        valueNodes[^1].Add(jsonPropertyNode.PropertyName, new(jsonPropertyNode.Value, jsonPropertyNode.ValueType));
                    }
                }
            }
        }
        return selectableJsonNode;
    }

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
