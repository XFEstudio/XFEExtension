using System.Text.Json;
using XFE各类拓展.NetCore.FormatExtension;

namespace XFE各类拓展.NetCore.ProfileExtension;

/// <summary>
/// XFE配置文件，实现配置文件读写自动化
/// </summary>
public abstract class XFEProfile
{
    private static Func<ProfileEntryInfo, string> SaveProfilesFunc { get; set; } = p => JsonSerializer.Serialize(p.Property.GetValue(null));
    private static Func<string, ProfileEntryInfo, object?> LoadProfilesFunc { get; set; } = (x, p) => JsonSerializer.Deserialize(x, p.Property.PropertyType);

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
    public static void LoadProfiles(params ProfileInfo[] profileInfo)
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
                        profile.PropertiesInfo[i].Property.SetValue(null, LoadProfilesFunc(property.Content, propertyInfo));
                        continue;
                    }
                    foreach (var propertySecFind in propertyFileContent)
                    {
                        if (propertySecFind.Header == propertyInfo.Name)
                        {
                            profile.PropertiesInfo[i].Property.SetValue(null, LoadProfilesFunc(propertySecFind.Content, propertyInfo));
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="profileInfo"></param>
    /// <returns></returns>
    public static async Task LoadProfilesAsync(params ProfileInfo[] profileInfo) => await Task.Run(() => LoadProfiles(profileInfo));

    /// <summary>
    /// 储存指定的配置文件
    /// </summary>
    /// <param name="profileInfo">配置文件</param>
    /// <returns></returns>
    public static void SaveProfile(ProfileInfo profileInfo)
    {
        var waitSaveProfile = Profiles.Find(x => x.Profile == profileInfo.Profile);
        if (waitSaveProfile is null)
            return;
        var saveProfileDictionary = new XFEDictionary();
        foreach (var property in waitSaveProfile.PropertiesInfo)
            saveProfileDictionary.Add(property.Name, SaveProfilesFunc(property));
        var fileSavePath = Path.GetDirectoryName(waitSaveProfile.Path);
        if (!Directory.Exists(fileSavePath) && fileSavePath is not null && fileSavePath != string.Empty)
            Directory.CreateDirectory(fileSavePath);
        File.WriteAllText(waitSaveProfile.Path, saveProfileDictionary);
    }

    /// <summary>
    /// 储存配置文件
    /// </summary>
    /// <returns></returns>
    public static void SaveProfiles()
    {
        foreach (var profile in Profiles)
            SaveProfile(profile);
    }

    /// <summary>
    /// 储存指定的配置文件
    /// </summary>
    /// <param name="profileInfo">配置文件</param>
    /// <returns></returns>
    public static async Task SaveProfileAsync(ProfileInfo profileInfo)
    {
        var waitSaveProfile = Profiles.Find(x => x.Profile == profileInfo.Profile);
        if (waitSaveProfile is null)
            return;
        var saveProfileDictionary = new XFEDictionary();
        foreach (var property in waitSaveProfile.PropertiesInfo)
            saveProfileDictionary.Add(property.Name, SaveProfilesFunc(property));
        var fileSavePath = Path.GetDirectoryName(waitSaveProfile.Path);
        if (!Directory.Exists(fileSavePath) && fileSavePath is not null && fileSavePath != string.Empty)
            Directory.CreateDirectory(fileSavePath);
        await File.WriteAllTextAsync(waitSaveProfile.Path, saveProfileDictionary);
    }

    /// <summary>
    /// 储存配置文件
    /// </summary>
    /// <returns></returns>
    public static async Task SaveProfilesAsync()
    {
        foreach (var profile in Profiles)
            await SaveProfileAsync(profile);
    }

    /// <summary>
    /// 设置储存配置文件的方法
    /// </summary>
    /// <param name="saveProfilesFunc">储存方法</param>
    public static void SetSaveProfilesFunction(Func<ProfileEntryInfo, string> saveProfilesFunc) => SaveProfilesFunc = saveProfilesFunc;

    /// <summary>
    /// 设置加载配置文件的方法
    /// </summary>
    /// <param name="loadProfilesFunc">加载方法</param>
    public static void SetLoadProfilesFunction(Func<string, ProfileEntryInfo, object?> loadProfilesFunc) => LoadProfilesFunc = loadProfilesFunc;

    /// <summary>
    /// 可写在属性的set访问器后，用于自动储存
    /// </summary>
    /// <param name="profileInfo"></param>
    /// <returns></returns>
    protected static void AutoSave(ProfileInfo profileInfo) => SaveProfile(profileInfo);

    /// <summary>
    /// 可写在属性的set访问器后，用于自动储存
    /// </summary>
    /// <param name="profileInfo"></param>
    /// <returns></returns>
    protected static async Task AutoSaveAsync(ProfileInfo profileInfo) => await SaveProfileAsync(profileInfo);
}
