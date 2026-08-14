using System.Diagnostics.CodeAnalysis;
using NewJson = XFEExtension.NetCore.XFETransform.Json;

namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// 旧版可查询 JSON 节点兼容包装器。新代码应使用 <see cref="NewJson.XFEJsonNode"/>。
/// </summary>
[Obsolete("请使用 XFEJsonNode。QueryableJsonNode 将在下一个主版本中删除。")]
public class QueryableJsonNode : INodeBase, IQueryableJsonNode
{
    private readonly NewJson.XFEJsonNode? _lazyNode;
    private JsonNode? _originalNode;
    private int _layer;
    private string _propertyName = string.Empty;

    /// <summary>
    /// 使用旧版节点创建兼容包装器。
    /// </summary>
    public QueryableJsonNode(JsonNode jsonNode)
    {
        _originalNode = jsonNode ?? throw new ArgumentNullException(nameof(jsonNode));
        _layer = jsonNode.Layer;
        _propertyName = jsonNode.PropertyName;
    }

    internal QueryableJsonNode(NewJson.XFEJsonNode node, int layer = -1, string propertyName = "")
    {
        _lazyNode = node;
        _layer = layer;
        _propertyName = propertyName;
    }

    /// <summary>
    /// 空节点。
    /// </summary>
    public static readonly QueryableJsonNode Empty = new(new JsonNode());

    /// <summary>
    /// 按属性路径查询当前节点，或执行旧版 package 指令。
    /// </summary>
    public QueryableJsonNode? this[params string[] nodeProperties]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(nodeProperties);
            if (nodeProperties.Length == 0)
                throw new ArgumentException("至少需要一个属性名称或打包指令。", nameof(nodeProperties));

            if (_lazyNode is null)
                return OriginalNode is JsonComplexPropertyNode complex
                    ? JsonNodeConverter.AnalyzePropertyArray(nodeProperties, complex)
                    : throw new InvalidOperationException("该节点为简单节点，无法继续查找。");

            if (nodeProperties[0].StartsWith("package:", StringComparison.OrdinalIgnoreCase))
                return Package(nodeProperties[0][8..], nodeProperties[1..]);

            var current = _lazyNode;
            var layer = _layer;
            string propertyName = _propertyName;
            foreach (var name in nodeProperties)
            {
                current = current[name];
                if (current is null)
                    return null;
                layer++;
                propertyName = name;
            }
            return new(current, layer, propertyName);
        }
    }

    /// <summary>
    /// 原始旧版节点。访问此属性会显式物化当前惰性子树。
    /// </summary>
    public JsonNode OriginalNode => _originalNode ??= JsonNodeConverter.Materialize(_lazyNode!, _propertyName, _layer);

    /// <summary>
    /// 节点字符串值；对象和数组返回空字符串。
    /// </summary>
    public string Value => _lazyNode is null
        ? OriginalNode is IValueNode valueNode ? valueNode.Value : string.Empty
        : JsonNodeConverter.GetCompatibilityValue(_lazyNode);

    internal object? InnerValue { get; set; }

    /// <summary>
    /// 标量数组的字符串列表。
    /// </summary>
    public List<string> List => GetList();

    /// <summary>
    /// 旧版值类型。
    /// </summary>
    public ValueType ValueType => _lazyNode is null
        ? OriginalNode is IValueNode valueNode ? valueNode.ValueType : ValueType.None
        : JsonNodeConverter.GetCompatibilityValueType(_lazyNode);

    /// <summary>
    /// 获取或设置兼容层级。
    /// </summary>
    public int Layer
    {
        get => _layer;
        set
        {
            _layer = value;
            if (_originalNode is not null)
                _originalNode.Layer = value;
        }
    }

    /// <summary>
    /// 获取或设置兼容属性名称。
    /// </summary>
    public string PropertyName
    {
        get => _propertyName;
        set
        {
            _propertyName = value;
            if (_originalNode is not null)
                _originalNode.PropertyName = value;
        }
    }

    /// <summary>
    /// 返回兼容字符串值。
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// 获取当前标量值。
    /// </summary>
    public object? GetValue() => _lazyNode is not null
        ? ValueType switch
        {
            ValueType.None or ValueType.Null => null,
            ValueType.Text => _lazyNode.GetString(),
            ValueType.Boolean => _lazyNode.GetBoolean(),
            ValueType.Int => _lazyNode.GetInt32(),
            ValueType.Long => _lazyNode.GetInt64(),
            ValueType.Float => _lazyNode.GetSingle(),
            _ => null
        }
        : OriginalNode is IValueNode valueNode
            ? GetValueByValueType(valueNode.Value, valueNode.ValueType)
            : null;

    /// <summary>
    /// 按旧版值类型转换字符串。
    /// </summary>
    public static object? GetValueByValueType(string value, ValueType valueType) => valueType switch
    {
        ValueType.None or ValueType.Null => null,
        ValueType.Text => value,
        ValueType.Char => char.Parse(value),
        ValueType.Boolean => bool.Parse(value),
        ValueType.Int => int.Parse(value, System.Globalization.CultureInfo.InvariantCulture),
        ValueType.Long => long.Parse(value, System.Globalization.CultureInfo.InvariantCulture),
        ValueType.Float => float.Parse(value, System.Globalization.CultureInfo.InvariantCulture),
        _ => throw new NotSupportedException($"不支持旧版值类型 {valueType}。")
    };

    /// <summary>
    /// 将当前节点转换为指定类型。
    /// </summary>
    public T GetValue<T>() => _lazyNode is not null ? _lazyNode.Deserialize<T>()! : (T)GetValue()!;

    /// <summary>
    /// 获取标量数组的字符串列表。
    /// </summary>
    public List<string> GetList() => _lazyNode is not null
        ? [.. _lazyNode.EnumerateArray().Select(JsonNodeConverter.GetCompatibilityValue)]
        : OriginalNode is JsonComplexPropertyNode complex
            ? [.. complex.DescendingNodes.Select(node => node is IValueNode valueNode ? valueNode.Value : null!)]
            : throw new InvalidOperationException("当前节点不是数组。");

    /// <summary>
    /// 获取指定类型的数组值。
    /// </summary>
    public List<T> GetList<T>() => _lazyNode is not null
        ? [.. _lazyNode.EnumerateArray().Select(node => node.Deserialize<T>()!)]
        : OriginalNode is JsonComplexPropertyNode complex
            ? [.. complex.DescendingNodes.Select(node => node is IValueNode valueNode ? (T)GetValueByValueType(valueNode.Value, valueNode.ValueType)! : default!)]
            : throw new InvalidOperationException("当前节点不是数组。");

    /// <summary>
    /// 获取旧版数组对象投影结果。
    /// </summary>
    [Obsolete("请使用 XFEJsonNode.ProjectArray。")]
    public List<Dictionary<string, ValueNode>> PackageInListObject() =>
        InnerValue as List<Dictionary<string, ValueNode>> ?? throw new InvalidOperationException("当前节点不包含 package:list 结果。");

    /// <summary>
    /// 获取旧版对象投影结果。
    /// </summary>
    [Obsolete("请使用 XFEJsonNode.SelectProperties。")]
    public Dictionary<string, ValueNode> PackageObject() =>
        InnerValue as Dictionary<string, ValueNode> ?? throw new InvalidOperationException("当前节点不包含 package:object 结果。");

    /// <summary>
    /// 获取直接子节点。
    /// </summary>
    public IEnumerable<QueryableJsonNode> GetChildNodes()
    {
        if (_lazyNode is not null)
        {
            return _lazyNode.Kind switch
            {
                NewJson.XFEJsonValueKind.Array => _lazyNode.EnumerateArray().Select((node, index) => new QueryableJsonNode(node, _layer + 1, index.ToString())),
                NewJson.XFEJsonValueKind.Object => _lazyNode.EnumerateObject().Select(property => new QueryableJsonNode(property.Value, _layer + 1, property.Name)),
                _ => throw new InvalidOperationException("无法枚举标量节点的子节点。")
            };
        }
        return OriginalNode is JsonComplexPropertyNode complex
            ? complex.DescendingNodes.Select(node => new QueryableJsonNode(node))
            : throw new InvalidOperationException("无法枚举标量节点的子节点。");
    }

    /// <summary>
    /// 隐式将 JSON 字符串转换为惰性兼容节点。
    /// </summary>
    [return: NotNullIfNotNull(nameof(jsonString))]
    public static implicit operator QueryableJsonNode?(string? jsonString) =>
        jsonString is null ? null : new(NewJson.XFEJson.Parse(jsonString));

    /// <summary>
    /// 链式查询属性。
    /// </summary>
    public static QueryableJsonNode? operator >(QueryableJsonNode? node, string propertyName) => node?[propertyName];
    /// <summary>
    /// 链式查询属性。
    /// </summary>
    public static QueryableJsonNode? operator <(QueryableJsonNode? node, string propertyName) => node?[propertyName];

    /// <summary>
    /// 隐式读取兼容字符串值。
    /// </summary>
    [return: NotNullIfNotNull(nameof(node))]
    public static implicit operator string?(QueryableJsonNode? node) => node?.ToString();

    private QueryableJsonNode? Package(string packageType, string[] propertyNames)
    {
        if (_lazyNode is null)
            return null;
        if (packageType.Equals("object", StringComparison.OrdinalIgnoreCase))
        {
            var result = new Dictionary<string, ValueNode>(StringComparer.Ordinal);
            foreach (var pair in _lazyNode.SelectProperties(propertyNames))
            {
                if (pair.Value is null || pair.Value.Kind is NewJson.XFEJsonValueKind.Object or NewJson.XFEJsonValueKind.Array)
                    continue;
                result[pair.Key] = new(JsonNodeConverter.GetCompatibilityValue(pair.Value), JsonNodeConverter.GetCompatibilityValueType(pair.Value));
            }
            return new QueryableJsonNode(_lazyNode, _layer, _propertyName) { InnerValue = result };
        }
        if (packageType.Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            var result = new List<Dictionary<string, ValueNode>>();
            foreach (var projection in _lazyNode.ProjectArray(propertyNames))
            {
                var item = new Dictionary<string, ValueNode>(StringComparer.Ordinal);
                foreach (var pair in projection)
                {
                    if (pair.Value is null || pair.Value.Kind is NewJson.XFEJsonValueKind.Object or NewJson.XFEJsonValueKind.Array)
                        continue;
                    item[pair.Key] = new(JsonNodeConverter.GetCompatibilityValue(pair.Value), JsonNodeConverter.GetCompatibilityValueType(pair.Value));
                }
                result.Add(item);
            }
            return new QueryableJsonNode(_lazyNode, _layer, _propertyName) { InnerValue = result };
        }
        return null;
    }
}
