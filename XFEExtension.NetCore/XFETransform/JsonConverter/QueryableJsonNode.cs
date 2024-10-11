namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// 可查询节点
/// </summary>
/// <param name="jsonNode"></param>
public class QueryableJsonNode(JsonNode jsonNode) : INodeBase, IQueryableJsonNode
{
    ///<inheritdoc/>
    public QueryableJsonNode this[params string[] nodeProperties] => OriginalNode is JsonComplexPropertyNode complexPropertyNode ? complexPropertyNode[nodeProperties] : throw new InvalidOperationException("该节点为简单节点，无法继续查找");
    /// <summary>
    /// 原始节点
    /// </summary>
    public JsonNode OriginalNode { get; } = jsonNode;
    /// <summary>
    /// 节点值
    /// </summary>
    public object? Value { get => GetValue(); }
    internal object? InnerValue { get; set; }
    /// <summary>
    /// 节点列表
    /// </summary>
    public List<string> List { get => GetList(); }
    /// <summary>
    /// 值类型
    /// </summary>
    public ValueType ValueType { get => OriginalNode is IValueNode valueNode ? valueNode.ValueType : ValueType.None; }
    ///<inheritdoc/>
    public int Layer { get => OriginalNode.Layer; set => OriginalNode.Layer = value; }
    ///<inheritdoc/>
    public string PropertyName { get => OriginalNode.PropertyName; set => OriginalNode.PropertyName = value; }
    /// <summary>
    /// 获取当前节点值
    /// </summary>
    /// <returns>节点值</returns>
    public object? GetValue() => OriginalNode is IValueNode valueNode ? GetValueByValueType(valueNode.Value, valueNode.ValueType) : null;
    /// <summary>
    /// 通过值类型获取值
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="valueType">值类型</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static object? GetValueByValueType(string value, ValueType valueType) => valueType switch
    {
        ValueType.None => null,
        ValueType.Text => value,
        ValueType.Char => char.Parse(value),
        ValueType.Boolean => bool.Parse(value),
        ValueType.Int => int.Parse(value),
        ValueType.Long => long.Parse(value),
        ValueType.Float => float.Parse(value),
        _ => throw new NotImplementedException()
    };
    /// <summary>
    /// 使用指定类型获取值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns></returns>
    public T GetValue<T>() => (T)Value!;
    /// <summary>
    /// 获取列表
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public List<string> GetList() => OriginalNode is JsonComplexPropertyNode jsonComplexPropertyNode ? jsonComplexPropertyNode.DescendingNodes.Select(node => node is IValueNode valueNode ? valueNode.Value : null!).ToList() : throw new NullReferenceException();
    /// <summary>
    /// 使用指定类型获取列表
    /// </summary>
    /// <typeparam name="T">指定类型</typeparam>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public List<T> GetList<T>() => OriginalNode is JsonComplexPropertyNode jsonComplexPropertyNode ? jsonComplexPropertyNode.DescendingNodes.Select(node => node is IValueNode valueNode ? (T)GetValueByValueType(valueNode.Value, valueNode.ValueType)! : default!).ToList() : throw new NullReferenceException();
    /// <summary>
    /// 对列表内对象属性值进行打包
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public List<Dictionary<string, ValueNode>> PackageInListObject() => InnerValue is List<Dictionary<string, ValueNode>> innerValue ? innerValue : throw new NullReferenceException();
    /// <summary>
    /// 对对象内对象属性值进行打包
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public Dictionary<string, ValueNode> PackageObject() => InnerValue is Dictionary<string, ValueNode> innerValue ? innerValue : throw new NullReferenceException();
    /// <summary>
    /// 隐式将Json字符串转换为可查询节点
    /// </summary>
    /// <param name="jsonString"></param>
    public static implicit operator QueryableJsonNode(string jsonString) => new(new JsonNodeConverter(jsonString).ConvertToJsonNode());
    /// <summary>
    /// 隐式将Json字符串转换为节点值
    /// </summary>
    /// <param name="queryableJsonNode"></param>
    public static implicit operator string(QueryableJsonNode queryableJsonNode) => $"{queryableJsonNode.Value}";
}