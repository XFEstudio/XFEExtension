namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// 节点根
/// </summary>
public interface INodeBase
{
    /// <summary>
    /// 节点层级
    /// </summary>
    int Layer { get; set; }
    /// <summary>
    /// 节点属性名称
    /// </summary>
    string PropertyName { get; set; }
}
