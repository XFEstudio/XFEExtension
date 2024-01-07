using System.Reflection;

namespace XFE各类拓展.NetCore.ProfileExtension;

/// <summary>
/// 配置文件信息
/// </summary>
/// <param name="profileType">配置文件类型</param>
/// <param name="path">配置文件储存位置</param>
/// <param name="description">配置文件描述</param>
public class ProfileInfo(Type profileType, string path = "", string description = "")
{
    /// <summary>
    /// 配置文件类型
    /// </summary>
    public Type Profile { get; init; } = profileType;
    /// <summary>
    /// 配置文件储存位置
    /// </summary>
    public string Path { get; init; } = path == "" ? $"{((XFEProfile.ProfilesRootPath[^1] == '/' || XFEProfile.ProfilesRootPath[^1] == '\\') ? $"{XFEProfile.ProfilesRootPath}{profileType.Name}.xfe" : $"{XFEProfile.ProfilesRootPath}/{profileType.Name}.xfe")}" : path;
    /// <summary>
    /// 配置文件描述
    /// </summary>
    public string? Description { get; set; } = description;
    /// <summary>
    /// 配置文件属性列表
    /// </summary>
    public List<ProfileEntryInfo> PropertiesInfo { get; init; } = GetPropertiesWithProfileAttribute(profileType);
    internal static List<ProfileEntryInfo> GetPropertiesWithProfileAttribute(Type type)
    {
        var profileEntryList = new List<ProfileEntryInfo>();
        foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            if (propertyInfo.GetCustomAttribute<ProfilePropertyAttribute>() is not null)
                profileEntryList.Add(new ProfileEntryInfo(propertyInfo.Name, propertyInfo));
        return profileEntryList;
    }
    /// <summary>
    /// 配置文件类型生成
    /// </summary>
    /// <param name="profileType"></param>
    public static implicit operator ProfileInfo(Type profileType) => new(profileType);
}