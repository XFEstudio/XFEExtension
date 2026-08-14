namespace XFEExtension.NetCore.XFETransform.JsonConverter;

/// <summary>
/// 值节点
/// </summary>
[Obsolete("请使用 XFEJsonNode。旧节点接口将在下一个主版本中删除。")]
public interface IValueNode
{
    /// <summary>
    /// 值
    /// </summary>
    string Value { get; set; }
    /// <summary>
    /// 值类型
    /// </summary>
    ValueType ValueType { get; set; }
}
