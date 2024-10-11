namespace XUnitConsole;

/// <summary>
/// 值节点
/// </summary>
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
