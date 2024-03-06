namespace XFE各类拓展.NetCore.ProfileExtension;

/// <summary>
/// 自动加载配置文件
/// </summary>
/// <param name="autoLoad">是否自动加载</param>
[AttributeUsage(AttributeTargets.Class)]
public class AutoLoadProfileAttribute(bool autoLoad = true) : Attribute
{
    /// <summary>
    /// 是否自动加载
    /// </summary>
    public bool AutoLoad { get; set; } = autoLoad;
}
