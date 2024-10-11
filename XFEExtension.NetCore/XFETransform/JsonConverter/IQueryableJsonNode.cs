namespace XUnitConsole;

/// <summary>
/// 可查询Json节点
/// </summary>
public interface IQueryableJsonNode
{
    /// <summary>
    /// 查询节点
    /// </summary>
    /// <param name="nodeProperties">查询属性名称或指定操作</param>
    /// <returns>下一个可查询节点</returns>
    public QueryableJsonNode this[params string[] nodeProperties] { get; }
}
