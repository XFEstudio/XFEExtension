namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// Json节点转换器
/// </summary>
/// <param name="jsonString"></param>
public class JsonNodeConverter(string jsonString)
{
    private int currentLayer = -1;
    private string currentPropertyName = "";
    private string currentPropertyValue = "";
    private readonly List<short> currentPosition = [0];
    private string jsonString = jsonString;
    /// <summary>
    /// 原始Json字符串
    /// </summary>
    public string JsonString
    {
        get { return jsonString; }
        set { jsonString = value; }
    }
    /// <summary>
    /// 转化为Json节点
    /// </summary>
    /// <returns></returns>
    public JsonNode ConvertToJsonNode()
    {
        var jsonNode = new JsonComplexPropertyNode();
        CurrentState currentState = CurrentState.None;
        JsonSymbol lastSymbol = JsonSymbol.Brace;
        for (int i = 0; i < jsonString.Length; i++)
        {
            var current = jsonString[i];
            var currentJsonComplexJsonNode = GetJsonNodeByIndex(jsonNode, currentPosition);
            JsonSymbol currentSymbol;
            ValueType currentValueType;
            switch (current)
            {
                case '{':
                    currentSymbol = JsonSymbol.Brace;
                    currentState = CurrentState.Property;
                    if (jsonNode.DescendingNodes.Count == 0)
                        break;
                    currentLayer++;
                    if (currentPosition.Count < currentLayer + 2)
                        currentPosition.Add(0);
                    else
                        currentPosition[currentLayer + 1] += 1;
                    var newObjectNode = new JsonComplexPropertyNode
                    {
                        DescendingNodes = [],
                        IsList = false,
                        Layer = currentLayer,
                        PropertyName = currentPropertyName.Replace("\"", "")
                    };
                    currentJsonComplexJsonNode.DescendingNodes.Add(newObjectNode);
                    currentPropertyName = "";
                    currentPropertyValue = "";
                    break;
                case '}':
                    currentSymbol = JsonSymbol.InvertedBrace;
                    currentState = CurrentState.None;
                    if (lastSymbol == JsonSymbol.Text || lastSymbol == JsonSymbol.DoubleQuotationMark)
                    {
                        currentValueType = JudgeValueType(ref currentPropertyValue);
                        currentJsonComplexJsonNode = GetJsonNodeByIndex(jsonNode, currentPosition);
                        if (SummonJsonPropertyNode(currentValueType, currentLayer, currentPropertyName.Replace("\"", ""), currentPropertyValue) is JsonPropertyNode jsonPropertyNode)
                            currentJsonComplexJsonNode.DescendingNodes.Add(jsonPropertyNode);
                        else
                            Console.WriteLine($"设置复杂Json节点失败：{currentLayer}\t{currentPropertyName}\t{currentPropertyValue}");
                    }
                    currentPosition.RemoveAt(currentPosition.Count - 1);
                    currentLayer--;
                    currentPropertyName = "";
                    currentPropertyValue = "";
                    break;
                case '[':
                    currentLayer++;
                    currentSymbol = JsonSymbol.Bracket;
                    currentState = CurrentState.ListValue;
                    if (currentPosition.Count < currentLayer + 2)
                        currentPosition.Add(0);
                    else
                        currentPosition[currentLayer + 1] += 1;
                    var newListNode = new JsonComplexPropertyNode
                    {
                        DescendingNodes = [],
                        IsList = true,
                        Layer = currentLayer,
                        PropertyName = currentPropertyName.Replace("\"", "")
                    };
                    currentJsonComplexJsonNode.DescendingNodes.Add(newListNode);
                    currentPropertyName = "";
                    currentPropertyValue = "";
                    break;
                case ']':
                    currentSymbol = JsonSymbol.InvertedBracket;
                    currentState = CurrentState.None;
                    if (lastSymbol == JsonSymbol.Text || lastSymbol == JsonSymbol.DoubleQuotationMark)
                    {
                        currentValueType = JudgeValueType(ref currentPropertyValue);
                        currentJsonComplexJsonNode = GetJsonNodeByIndex(jsonNode, currentPosition);
                        if (SummonJsonPropertyNode(currentValueType, currentLayer, currentPropertyName.Replace("\"", ""), currentPropertyValue) is JsonPropertyNode jsonPropertyNode)
                            currentJsonComplexJsonNode.DescendingNodes.Add(jsonPropertyNode);
                        else
                            Console.WriteLine($"设置复杂Json节点失败：{currentLayer}\t{currentPropertyName}\t{currentPropertyValue}");
                    }
                    currentPosition.RemoveAt(currentPosition.Count - 1);
                    currentLayer--;
                    break;
                case ':':
                    currentSymbol = JsonSymbol.Colon;
                    currentState = CurrentState.PropertyValue;
                    break;
                case ',':
                    currentSymbol = JsonSymbol.Comma;
                    currentPosition[currentLayer + 1] += 1;
                    if (currentState != CurrentState.ListValue)
                        currentState = CurrentState.Property;
                    if (lastSymbol == JsonSymbol.Text || lastSymbol == JsonSymbol.DoubleQuotationMark)
                    {
                        currentValueType = JudgeValueType(ref currentPropertyValue);
                        currentJsonComplexJsonNode = GetJsonNodeByIndex(jsonNode, currentPosition);
                        if (SummonJsonPropertyNode(currentValueType, currentLayer, currentPropertyName.Replace("\"", ""), currentPropertyValue) is JsonPropertyNode jsonPropertyNode)
                            currentJsonComplexJsonNode.DescendingNodes.Add(jsonPropertyNode);
                        else
                            Console.WriteLine($"设置复杂Json节点失败：{currentLayer}\t{currentPropertyName}\t{currentPropertyValue}");
                    }
                    currentPropertyName = "";
                    currentPropertyValue = "";
                    break;
                case '"':
                    currentSymbol = JsonSymbol.DoubleQuotationMark;
                    if (currentState == CurrentState.Property)
                        currentPropertyName += "\"";
                    else if (currentState == CurrentState.PropertyValue)
                        currentPropertyValue += "\"";
                    break;
                case '\'':
                    currentSymbol = JsonSymbol.QuotationMark;
                    currentState = CurrentState.None;
                    break;
                default:
                    currentSymbol = JsonSymbol.Text;
                    switch (currentState)
                    {
                        case CurrentState.Property:
                            currentPropertyName += current;
                            break;
                        case CurrentState.PropertyValue:
                            currentPropertyValue += current;
                            break;
                        case CurrentState.ListValue:
                            currentPropertyValue += current;
                            break;
                        case CurrentState.None:
                            break;
                        default:
                            break;
                    }
                    break;
            }
            lastSymbol = currentSymbol;
        }
        return jsonNode;
    }
    private static JsonComplexPropertyNode GetJsonNodeByIndex(JsonComplexPropertyNode jsonComplexPropertyNode, List<short> indexes)
    {
        var currentJsonComplexPropertyNode = jsonComplexPropertyNode;
        for (int i = 0; i < indexes.Count - 1; i++)
        {
            short index = indexes[i];
            if (currentJsonComplexPropertyNode.DescendingNodes[index] is JsonComplexPropertyNode complexPropertyNode)
            {
                currentJsonComplexPropertyNode = complexPropertyNode;
            }
        }
        return currentJsonComplexPropertyNode;
    }
    private static ValueType JudgeValueType(ref string currentPropertyValue)
    {
        if (currentPropertyValue.StartsWith('"'))
        {
            currentPropertyValue = currentPropertyValue.Replace("\"", "");
            return ValueType.Text;
        }
        else if (currentPropertyValue.StartsWith('\''))
        {
            currentPropertyValue = currentPropertyValue.Replace("'", "");
            return ValueType.Char;
        }
        else if (currentPropertyValue.Contains('.'))
        {
            return ValueType.Float;
        }
        else if (bool.TryParse(currentPropertyValue, out _))
        {
            return ValueType.Boolean;
        }
        else if (int.TryParse(currentPropertyValue, out _))
        {
            return ValueType.Int;
        }
        else
        {
            return ValueType.Long;
        }
    }
    private static JsonPropertyNode? SummonJsonPropertyNode(ValueType currentValueType, int currentLayer, string currentPropertyName, string currentPropertyValue)
    {
        var jsonPropertyNode = new JsonPropertyNode
        {
            Layer = currentLayer,
            PropertyName = currentPropertyName,
            Value = currentPropertyValue,
            ValueType = currentValueType
        };
        return jsonPropertyNode;
    }
    internal static QueryableJsonNode AnalyzePropertyArray(string[] nodeProperties, JsonComplexPropertyNode jsonComplexPropertyNode)
    {
        if (nodeProperties[0].StartsWith("package:", StringComparison.CurrentCultureIgnoreCase))
        {
            return nodeProperties[0][8..].ToLowerInvariant() switch
            {
                "list" => NodeOperator.PackageInList(nodeProperties[1..], jsonComplexPropertyNode),
                "object" => NodeOperator.PackageToList(nodeProperties[1..], jsonComplexPropertyNode),
                _ => throw new InvalidOperationException()
            };
        }
        else
        {
            return jsonComplexPropertyNode.DescendingNodes.Find(node => node.PropertyName == nodeProperties[0]) is JsonNode jsonNode ? new(jsonNode) : throw new InvalidOperationException();
        }
    }
}
enum CurrentState
{
    Property,
    PropertyValue,
    ListValue,
    None
}
enum JsonSymbol
{
    Brace,
    InvertedBrace,
    Bracket,
    InvertedBracket,
    Comma,
    Colon,
    QuotationMark,
    DoubleQuotationMark,
    Text
}
