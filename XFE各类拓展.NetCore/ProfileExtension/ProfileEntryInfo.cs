using System.Reflection;

namespace XFE各类拓展.NetCore.ProfileExtension;

/// <summary>
/// 配置文件属性信息
/// </summary>
/// <param name="name">属性名称</param>
/// <param name="propertyInfo">属性信息</param>
public class ProfileEntryInfo(string name, PropertyInfo propertyInfo)
{
    /// <summary>
    /// 属性名称
    /// </summary>
    public string Name { get; set; } = name;
    /// <summary>
    /// 属性值
    /// </summary>
    public string? Value { get { return Property.GetValue(null)?.ToString(); } }
    /// <summary>
    /// 属性信息
    /// </summary>
    public PropertyInfo Property { get; init; } = propertyInfo;
}
