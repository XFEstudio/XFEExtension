namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// Json节点
/// </summary>
[Obsolete("请使用 XFEJsonNode。旧节点树将在下一个主版本中删除。")]
public class JsonNode : INodeBase
{
    /// <summary>
    /// 获取或设置当前 JSON 节点在节点树中的层级深度。
    /// </summary>
    public int Layer { get; set; }
    /// <summary>
    /// 获取或设置当前 JSON 节点对应的属性名称。
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;
}
