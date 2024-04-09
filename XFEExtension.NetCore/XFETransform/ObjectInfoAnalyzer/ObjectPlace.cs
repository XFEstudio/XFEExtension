namespace XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

/// <summary>
/// 对象位置
/// </summary>
public enum ObjectPlace
{
    /// <summary>
    /// 属性
    /// </summary>
    Property,
    /// <summary>
    /// 字段
    /// </summary>
    Field,
    /// <summary>
    /// 主对象
    /// </summary>
    Main,
    /// <summary>
    /// 枚举
    /// </summary>
    Enum,
    /// <summary>
    /// 列表
    /// </summary>
    List,
    /// <summary>
    /// 列表成员
    /// </summary>
    ListMember,
    /// <summary>
    /// 数组
    /// </summary>
    Array,
    /// <summary>
    /// 数组成员
    /// </summary>
    ArrayMember,
    /// <summary>
    /// 其他
    /// </summary>
    Other
}