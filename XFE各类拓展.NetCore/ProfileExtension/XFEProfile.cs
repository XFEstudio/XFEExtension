using XFE各类拓展.NetCore.FormatExtension;

namespace XFE各类拓展.NetCore.ProfileExtension;

/// <summary>
/// XFE配置文件，实现配置文件读写自动化
/// </summary>
public abstract class XFEProfile
{
    /// <summary>
    /// 配置文件清单
    /// </summary>
    public static List<ProfileInfo> Profiles { get; private set; } = [];
    /// <summary>
    /// 配置文件所在的根目录
    /// </summary>
    public static string ProfilesRootPath { get; set; } = $"{AppDomain.CurrentDomain.BaseDirectory}/Profiles";
    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="profileInfo"></param>
    /// <returns></returns>
    public static async Task LoadProfiles(params ProfileInfo[] profileInfo)
    {
        await Task.Run(() =>
        {
            Profiles.AddRange(profileInfo);
            foreach (var profile in Profiles)
            {
                if (!File.Exists(profile.Path))
                    continue;
                XFEDictionary propertyFileContent = File.ReadAllText(profile.Path);
                for (int i = 0; i < profile.PropertiesInfo.Count; i++)
                {
                    for (int j = 0; j < propertyFileContent.Count; j++)
                    {
                        var propertyInfo = profile.PropertiesInfo[i];
                        var property = propertyFileContent.ElementAt(j);
                        if (property.Header == propertyInfo.Name)
                        {
                            profile.PropertiesInfo[i].Property.SetValue(null, Convert.ChangeType(property.Content, propertyInfo.Property.PropertyType));
                            continue;
                        }
                        foreach (var propertySecFind in propertyFileContent)
                        {
                            if (propertySecFind.Header == propertyInfo.Name)
                            {
                                profile.PropertiesInfo[i].Property.SetValue(null, Convert.ChangeType(propertySecFind.Content, propertyInfo.Property.PropertyType));
                                break;
                            }
                        }
                    }
                }
            }
        });
    }
    /// <summary>
    /// 储存指定的配置文件
    /// </summary>
    /// <param name="profileInfo">配置文件</param>
    /// <returns></returns>
    public static async Task SaveProfile(ProfileInfo profileInfo)
    {
        var waitSaveProfile = Profiles.Find(x => x.Profile == profileInfo.Profile);
        if (waitSaveProfile is null)
            return;
        var saveProfileDictionary = new XFEDictionary();
        foreach (var property in waitSaveProfile.PropertiesInfo)
            saveProfileDictionary.Add(property.Name, property.Value is null ? string.Empty : property.Value);
        var fileSavePath = Path.GetDirectoryName(waitSaveProfile.Path);
        if (!Directory.Exists(fileSavePath) && fileSavePath is not null && fileSavePath != string.Empty)
            Directory.CreateDirectory(fileSavePath);
        await File.WriteAllTextAsync(waitSaveProfile.Path, saveProfileDictionary);
    }
    /// <summary>
    /// 储存配置文件
    /// </summary>
    /// <returns></returns>
    public static async Task SaveProfiles()
    {
        foreach (var profile in Profiles)
            await SaveProfile(profile);
    }
    /// <summary>
    /// 可写在属性的set访问器后，用于自动储存
    /// </summary>
    /// <param name="profileInfo"></param>
    /// <returns></returns>
    protected static async Task AutoSave(ProfileInfo profileInfo) => await SaveProfile(profileInfo);
}
