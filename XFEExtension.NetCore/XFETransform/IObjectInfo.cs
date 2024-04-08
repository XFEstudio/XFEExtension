namespace XFEExtension.NetCore.XFETransform;

/// <summary>
/// 对象信息接口
/// </summary>
public interface IObjectInfo
{
    /// <summary>
    /// 是否是数组
    /// </summary>
    bool IsArray { get; init; }
    /// <summary>
    /// 是否是基础类型
    /// </summary>
    bool IsBasicType { get; init; }
    /// <summary>
    /// 对象名称
    /// </summary>
    string? Name { get; init; }
    /// <summary>
    /// 子对象
    /// </summary>
    ISubObjects? SubObjects { get; init; }
    /// <summary>
    /// 对象类型
    /// </summary>
    Type Type { get; init; }
    /// <summary>
    /// 对象值
    /// </summary>
    object? Value { get; init; }
    /// <summary>
    /// 对象位置
    /// </summary>
    ObjectPlace ObjectPlace { get; init; }
    /// <summary>
    /// 对象层级
    /// </summary>
    int Layer { get; init; }

    string AddObjectPlace();

    /// <summary>
    /// 输出对象信息
    /// </summary>
    /// <returns></returns>
    string OutPut();
}