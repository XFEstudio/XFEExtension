namespace XFEExtension.NetCore.PathExtension;

/// <summary>
/// 自动检测并生成对应文件夹
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class AutoPathAttribute : Attribute
{
    /// <summary>
    /// 生成的属性名称
    /// </summary>
    public string? PropertyName { get; set; }
    /// <summary>
    /// 自动检测并生成对应文件夹
    /// </summary>
    public AutoPathAttribute()
    {
    }
    /// <summary>
    /// 自动检测并生成对应文件夹
    /// </summary>
    /// <param name="propertyName"></param>
    public AutoPathAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}