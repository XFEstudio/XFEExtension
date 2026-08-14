namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// Json复杂节点
/// </summary>
[Obsolete("请使用 XFEJsonNode。旧节点树将在下一个主版本中删除。")]
public class JsonComplexPropertyNode : JsonNode, IQueryableJsonNode
{
    /// <summary>
    /// 按属性路径查询当前复杂 JSON 节点下的子节点。
    /// </summary>
    /// <param name="nodeProperties">从当前节点开始依次匹配的属性名称或打包指令。</param>
    /// <returns>路径对应的可查询节点；未找到对应路径时为 <see langword="null"/>。</returns>
    public QueryableJsonNode? this[params string[] nodeProperties] => JsonNodeConverter.AnalyzePropertyArray(nodeProperties, this);
    /// <summary>
    /// 是否是列表
    /// </summary>
    public bool IsList { get; set; }
    /// <summary>
    /// 子节点
    /// </summary>
    public List<JsonNode> DescendingNodes { get; set; } = [];
}
