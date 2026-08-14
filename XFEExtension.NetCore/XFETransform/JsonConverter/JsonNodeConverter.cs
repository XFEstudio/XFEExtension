using NewJson = XFEExtension.NetCore.XFETransform.Json;

namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// 旧版 JSON 节点树转换器。新代码应使用 <see cref="NewJson.XFEJson"/> 和
/// <see cref="NewJson.XFEJsonNode"/> 的惰性 API。
/// </summary>
/// <param name="jsonString">JSON 文本。</param>
[Obsolete("JsonNodeConverter 会显式物化完整兼容树。请使用 XFEJson.Parse 获取惰性 XFEJsonNode。")]
public class JsonNodeConverter(string jsonString)
{
    /// <summary>
    /// 原始 JSON 字符串。
    /// </summary>
    public string JsonString { get; set; } = jsonString;

    /// <summary>
    /// 将 JSON 显式物化为旧版节点树。
    /// </summary>
    public JsonNode ConvertToJsonNode()
    {
        ArgumentNullException.ThrowIfNull(JsonString);
        var root = NewJson.XFEJson.Parse(JsonString);
        root.Validate();
        return Materialize(root, string.Empty, -1);
    }

    internal static QueryableJsonNode? AnalyzePropertyArray(string[] nodeProperties, JsonComplexPropertyNode jsonComplexPropertyNode)
    {
        ArgumentNullException.ThrowIfNull(nodeProperties);
        if (nodeProperties.Length == 0)
            throw new ArgumentException("至少需要一个属性名称或打包指令。", nameof(nodeProperties));

        if (nodeProperties[0].StartsWith("package:", StringComparison.OrdinalIgnoreCase))
        {
            return nodeProperties[0][8..].ToLowerInvariant() switch
            {
                "list" => NodeOperator.PackageInList(nodeProperties[1..], jsonComplexPropertyNode),
                "object" => NodeOperator.PackageToList(nodeProperties[1..], jsonComplexPropertyNode),
                _ => null
            };
        }

        JsonNode current = jsonComplexPropertyNode;
        foreach (var propertyName in nodeProperties)
        {
            if (current is not JsonComplexPropertyNode complex)
                throw new InvalidOperationException("该节点为简单节点，无法继续查找。");
            current = complex.DescendingNodes.Find(node => node.PropertyName == propertyName)!;
            if (current is null)
                return null;
        }
        return new(current);
    }

    internal static JsonNode Materialize(NewJson.XFEJsonNode node, string propertyName, int layer)
    {
        switch (node.Kind)
        {
            case NewJson.XFEJsonValueKind.Object:
                {
                    var result = new JsonComplexPropertyNode
                    {
                        IsList = false,
                        Layer = layer,
                        PropertyName = propertyName
                    };
                    foreach (var property in node.EnumerateObject())
                        result.DescendingNodes.Add(Materialize(property.Value, property.Name, layer + 1));
                    return result;
                }
            case NewJson.XFEJsonValueKind.Array:
                {
                    var result = new JsonComplexPropertyNode
                    {
                        IsList = true,
                        Layer = layer,
                        PropertyName = propertyName
                    };
                    foreach (var element in node.EnumerateArray())
                        result.DescendingNodes.Add(Materialize(element, string.Empty, layer + 1));
                    return result;
                }
            default:
                return new JsonPropertyNode
                {
                    Layer = layer,
                    PropertyName = propertyName,
                    Value = GetCompatibilityValue(node),
                    ValueType = GetCompatibilityValueType(node)
                };
        }
    }

    internal static string GetCompatibilityValue(NewJson.XFEJsonNode node) => node.Kind switch
    {
        NewJson.XFEJsonValueKind.String => node.GetString() ?? string.Empty,
        NewJson.XFEJsonValueKind.Null => string.Empty,
        _ => node.GetRawText()
    };

    internal static ValueType GetCompatibilityValueType(NewJson.XFEJsonNode node)
    {
        return node.Kind switch
        {
            NewJson.XFEJsonValueKind.String => ValueType.Text,
            NewJson.XFEJsonValueKind.True or NewJson.XFEJsonValueKind.False => ValueType.Boolean,
            NewJson.XFEJsonValueKind.Null => ValueType.Null,
            NewJson.XFEJsonValueKind.Number => JudgeNumberType(node.GetRawText()),
            _ => ValueType.None
        };
    }

    private static ValueType JudgeNumberType(string rawText)
    {
        if (rawText.ContainsAny('.', 'e', 'E'))
            return ValueType.Float;
        if (int.TryParse(rawText, System.Globalization.NumberStyles.AllowLeadingSign,
                System.Globalization.CultureInfo.InvariantCulture, out _))
            return ValueType.Int;
        return long.TryParse(rawText, System.Globalization.NumberStyles.AllowLeadingSign,
            System.Globalization.CultureInfo.InvariantCulture, out _) ? ValueType.Long : ValueType.Float;
    }
}
