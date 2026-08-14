namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// 可查询Json节点
/// </summary>
[Obsolete("请使用 XFEJsonNode。旧节点接口将在下一个主版本中删除。")]
public interface IQueryableJsonNode
{
    /// <summary>
    /// 查询节点
    /// </summary>
    /// <param name="nodeProperties">查询属性名称或指定操作</param>
    /// <returns>下一个可查询节点</returns>
    public QueryableJsonNode? this[params string[] nodeProperties] { get; }
}
