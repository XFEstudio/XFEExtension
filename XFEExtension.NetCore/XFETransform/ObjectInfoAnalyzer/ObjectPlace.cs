namespace XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

/// <summary>
/// 对象位置
/// </summary>
public enum ObjectPlace
{
    /// <summary>
    /// 一般类型属性
    /// </summary>
    NormalProperty,
    /// <summary>
    /// 列表类型属性
    /// </summary>
    ListProperty,
    /// <summary>
    /// 数组类型属性
    /// </summary>
    ArrayProperty,
    /// <summary>
    /// 枚举类型属性
    /// </summary>
    EnumProperty,
    /// <summary>
    /// 一般类型字段
    /// </summary>
    NormalField,
    /// <summary>
    /// 列表类型字段
    /// </summary>
    ListField,
    /// <summary>
    /// 数组类型字段
    /// </summary>
    ArrayField,
    /// <summary>
    /// 枚举类型字段
    /// </summary>
    EnumField,
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