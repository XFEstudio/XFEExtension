namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// Json节点转换器
/// </summary>
/// <param name="jsonString"></param>
public class JsonNodeConverter(string jsonString)
{
    private int _currentLayer = -1;
    private string _currentPropertyName = "";
    private string _currentPropertyValue = "";
    private readonly List<short> _currentPosition = new List<short> { 0 };

    /// <summary>
    /// 原始Json字符串
    /// </summary>
    public string JsonString { get; set; } = jsonString;

    /// <summary>
    /// 转化为Json节点
    /// </summary>
    /// <returns></returns>
    public JsonNode ConvertToJsonNode()
    {
        var jsonNode = new JsonComplexPropertyNode();
        var currentState = CurrentState.None;
        var lastSymbol = JsonSymbol.Brace;
        var currentSymbol = JsonSymbol.Brace;
        foreach (var current in JsonString)
        {
            var currentJsonComplexJsonNode = GetJsonNodeByIndex(jsonNode, _currentPosition);
            ValueType currentValueType;
            switch (current)
            {
                case '{':
                    if ((currentState != CurrentState.Property && currentState != CurrentState.StringPropertyValue))
                    {
                        currentSymbol = JsonSymbol.Brace;
                        currentState = CurrentState.BeforeProperty;
                        if (jsonNode.DescendingNodes.Count == 0 && lastSymbol != JsonSymbol.Colon)
                            break;
                        _currentLayer++;
                        if (_currentPosition.Count < _currentLayer + 2)
                            _currentPosition.Add(0);
                        else
                            _currentPosition[_currentLayer + 1] += 1;
                        var newObjectNode = new JsonComplexPropertyNode
                        {
                            DescendingNodes = [],
                            IsList = false,
                            Layer = _currentLayer,
                            PropertyName = _currentPropertyName.Replace("\"", "")
                        };
                        currentJsonComplexJsonNode.DescendingNodes.Add(newObjectNode);
                        _currentPropertyName = "";
                        _currentPropertyValue = "";
                    }
                    else
                    {
                        _currentPropertyValue += "{";
                    }
                    break;
                case '}':
                    if ((currentState != CurrentState.Property && currentState != CurrentState.StringPropertyValue))
                    {
                        currentSymbol = JsonSymbol.InvertedBrace;
                        currentState = CurrentState.None;
                        if (lastSymbol is JsonSymbol.Text or JsonSymbol.DoubleQuotationMark)
                        {
                            currentValueType = JudgeValueType(ref _currentPropertyValue);
                            currentJsonComplexJsonNode = GetJsonNodeByIndex(jsonNode, _currentPosition);
                            if (SummonJsonPropertyNode(currentValueType, _currentLayer, _currentPropertyName.Replace("\"", ""), _currentPropertyValue) is { } jsonPropertyNode)
                                currentJsonComplexJsonNode.DescendingNodes.Add(jsonPropertyNode);
                            else
                                Console.WriteLine($"设置复杂Json节点失败：{_currentLayer}\t{_currentPropertyName}\t{_currentPropertyValue}");
                        }
                        // Never remove the root position (index 0). Only pop when there are >1 entries.
                        if (_currentPosition.Count > 1)
                        {
                            _currentPosition.RemoveAt(_currentPosition.Count - 1);
                            if (_currentLayer > -1)
                                _currentLayer--;
                        }
                        _currentPropertyName = "";
                        _currentPropertyValue = "";
                    }
                    else
                    {
                        _currentPropertyValue += "}";
                    }
                    break;
                case '[':
                    if ((currentState != CurrentState.Property && currentState != CurrentState.StringPropertyValue))
                    {
                        _currentLayer++;
                        currentSymbol = JsonSymbol.Bracket;
                        currentState = CurrentState.ListValue;
                        if (_currentPosition.Count < _currentLayer + 2)
                            _currentPosition.Add(0);
                        else
                            _currentPosition[_currentLayer + 1] += 1;
                        var newListNode = new JsonComplexPropertyNode
                        {
                            DescendingNodes = [],
                            IsList = true,
                            Layer = _currentLayer,
                            PropertyName = _currentPropertyName.Replace("\"", "")
                        };
                        currentJsonComplexJsonNode.DescendingNodes.Add(newListNode);
                        _currentPropertyName = "";
                        _currentPropertyValue = "";
                    }
                    else
                    {
                        _currentPropertyValue += "[";
                    }
                    break;
                case ']':
                    currentSymbol = JsonSymbol.InvertedBracket;
                    if ((currentState != CurrentState.Property && currentState != CurrentState.StringPropertyValue))
                    {
                        currentState = CurrentState.None;
                        if (lastSymbol is JsonSymbol.Text or JsonSymbol.DoubleQuotationMark)
                        {
                            currentValueType = JudgeValueType(ref _currentPropertyValue);
                            currentJsonComplexJsonNode = GetJsonNodeByIndex(jsonNode, _currentPosition);
                            if (SummonJsonPropertyNode(currentValueType, _currentLayer, _currentPropertyName.Replace("\"", ""), _currentPropertyValue) is { } jsonPropertyNode)
                                currentJsonComplexJsonNode.DescendingNodes.Add(jsonPropertyNode);
                            else
                                Console.WriteLine($"设置复杂Json节点失败：{_currentLayer}\t{_currentPropertyName}\t{_currentPropertyValue}");
                        }
                        // Never remove the root position (index 0). Only pop when there are >1 entries.
                        if (_currentPosition.Count > 1)
                        {
                            _currentPosition.RemoveAt(_currentPosition.Count - 1);
                            if (_currentLayer > -1)
                                _currentLayer--;
                        }
                    }
                    else
                    {
                        _currentPropertyValue += "]";
                    }
                    break;
                case ':':
                    if ((currentState != CurrentState.Property && currentState != CurrentState.StringPropertyValue))
                    {
                        currentSymbol = JsonSymbol.Colon;
                        currentState = CurrentState.BeforePropertyValue;
                    }
                    else
                    {
                        _currentPropertyValue += ":";
                    }
                    break;
                case ',':
                    if ((currentState != CurrentState.Property && currentState != CurrentState.StringPropertyValue))
                    {
                        currentSymbol = JsonSymbol.Comma;
                        if (_currentLayer >= -1)
                            _currentPosition[_currentLayer + 1] += 1;
                        if (currentState != CurrentState.ListValue)
                            currentState = CurrentState.BeforeProperty;
                        if (lastSymbol is JsonSymbol.Text or JsonSymbol.DoubleQuotationMark)
                        {
                            currentValueType = JudgeValueType(ref _currentPropertyValue);
                            currentJsonComplexJsonNode = GetJsonNodeByIndex(jsonNode, _currentPosition);
                            if (SummonJsonPropertyNode(currentValueType, _currentLayer, _currentPropertyName.Replace("\"", ""), _currentPropertyValue) is { } jsonPropertyNode)
                                currentJsonComplexJsonNode.DescendingNodes.Add(jsonPropertyNode);
                            else
                                Console.WriteLine($"设置复杂Json节点失败：{_currentLayer}\t{_currentPropertyName}\t{_currentPropertyValue}");
                        }
                        _currentPropertyName = "";
                        _currentPropertyValue = "";
                    }
                    else
                    {
                        _currentPropertyValue += ",";
                    }
                    break;
                case '"':
                    currentSymbol = JsonSymbol.DoubleQuotationMark;
                    switch (currentState)
                    {
                        case CurrentState.Property:
                            _currentPropertyName += "\"";
                            currentState = CurrentState.AfterPropertyValue;
                            break;
                        case CurrentState.BeforeProperty:
                            _currentPropertyName += "\"";
                            currentState = CurrentState.Property;
                            break;
                        case CurrentState.StringPropertyValue:
                            _currentPropertyValue += "\"";
                            currentState = CurrentState.AfterPropertyValue;
                            break;
                        case CurrentState.BeforePropertyValue:
                            _currentPropertyValue += "\"";
                            currentState = CurrentState.StringPropertyValue;
                            break;
                    }
                    break;
                case '\'':
                    currentSymbol = JsonSymbol.QuotationMark;
                    _currentPropertyName += "'";
                    currentState = CurrentState.None;
                    break;
                default:
                    switch (currentState)
                    {
                        case CurrentState.Property:
                            _currentPropertyName += current;
                            currentSymbol = JsonSymbol.Text;
                            break;
                        case CurrentState.PropertyValue or CurrentState.StringPropertyValue:
                        case CurrentState.ListValue:
                            _currentPropertyValue += current;
                            currentSymbol = JsonSymbol.Text;
                            break;
                        case CurrentState.BeforePropertyValue:
                            if (IsNormalValueExceptString(current))
                            {
                                _currentPropertyValue += current;
                                currentState = CurrentState.PropertyValue;
                                currentSymbol = JsonSymbol.Text;
                            }
                            break;
                        case CurrentState.None:
                            break;
                    }
                    break;
            }
            lastSymbol = currentSymbol;
        }
        return jsonNode;
    }
    private static bool IsNormalValueExceptString(char current) => current switch
    {
        '-' => true,
        '0' => true,
        '1' => true,
        '2' => true,
        '3' => true,
        '4' => true,
        '5' => true,
        '6' => true,
        '7' => true,
        '8' => true,
        '9' => true,
        'f' => true,
        'F' => true,
        't' => true,
        'T' => true,
        'n' => true,
        'N' => true,
        _ => false
    };
    private static JsonComplexPropertyNode GetJsonNodeByIndex(JsonComplexPropertyNode jsonComplexPropertyNode, List<short> indexes)
    {
        var currentJsonComplexPropertyNode = jsonComplexPropertyNode;
        for (var i = 0; i < indexes.Count - 1; i++)
        {
            var index = indexes[i];
            // Guard against invalid indexes to avoid ArgumentOutOfRangeException.
            if (index < 0 || index >= currentJsonComplexPropertyNode.DescendingNodes.Count)
            {
                // If the requested index is out of range, stop descending and return the current node.
                return currentJsonComplexPropertyNode;
            }

            if (currentJsonComplexPropertyNode.DescendingNodes[index] is JsonComplexPropertyNode complexPropertyNode)
            {
                currentJsonComplexPropertyNode = complexPropertyNode;
            }
            else
            {
                // If the node at the index is not a complex node, stop descending.
                return currentJsonComplexPropertyNode;
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

        if (currentPropertyValue.Equals("null", StringComparison.CurrentCultureIgnoreCase))
        {
            return ValueType.Null;
        }

        if (currentPropertyValue.StartsWith('\''))
        {
            currentPropertyValue = currentPropertyValue.Replace("'", "");
            return ValueType.Char;
        }

        if (currentPropertyValue.Contains('.'))
        {
            return ValueType.Float;
        }

        if (bool.TryParse(currentPropertyValue, out _))
        {
            return ValueType.Boolean;
        }

        if (int.TryParse(currentPropertyValue, out _))
        {
            return ValueType.Int;
        }

        return long.TryParse(currentPropertyValue, out _) ? ValueType.Long : ValueType.None;
    }
    private static JsonPropertyNode SummonJsonPropertyNode(ValueType currentValueType, int currentLayer, string currentPropertyName, string currentPropertyValue)
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
    internal static QueryableJsonNode? AnalyzePropertyArray(string[] nodeProperties, JsonComplexPropertyNode jsonComplexPropertyNode)
    {
        if (nodeProperties[0].StartsWith("package:", StringComparison.CurrentCultureIgnoreCase))
        {
            return nodeProperties[0][8..].ToLowerInvariant() switch
            {
                "list" => NodeOperator.PackageInList(nodeProperties[1..], jsonComplexPropertyNode),
                "object" => NodeOperator.PackageToList(nodeProperties[1..], jsonComplexPropertyNode),
                _ => null,
            };
        }

        return jsonComplexPropertyNode.DescendingNodes.Find(node => node.PropertyName == nodeProperties[0]) is { } jsonNode ? new(jsonNode) : null;
    }
}
enum CurrentState
{
    Property,
    BeforeProperty,
    AfterProperty,
    PropertyValue,
    StringPropertyValue,
    BeforePropertyValue,
    AfterPropertyValue,
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
