using XFEExtension.NetCore.FormatExtension;

namespace XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

/// <summary>
/// API密钥
/// </summary>
/// <param name="aPiKey">秘钥</param>
/// <param name="description">描述</param>
public class ApiKey(string aPiKey, string description)
{
    private static readonly char[] Separator = ['{', '}'];
    /// <summary>
    /// APIKEY
    /// </summary>
    public string Key { get; set; } = aPiKey;
    /// <summary>
    /// APIKey的描述
    /// </summary>
    public string Description { get; set; } = description;
    /// <summary>
    /// 转换为字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{{[+-{Description}-+][+-{Key}-+]}}";

    /// <summary>
    /// 将字符串转换为ApiKey的List
    /// </summary>
    /// <param name="keyString"></param>
    /// <returns></returns>
    public static List<ApiKey> ToApiKey(string keyString)
    {
        var keyGroup = keyString.Split(Separator);
        List<ApiKey> apiKeys = [];
        apiKeys.AddRange(from key in keyGroup select key.Split(XFEDictionary.EntrySeparator, StringSplitOptions.RemoveEmptyEntries) into oneKey where oneKey.Length == 2 select new ApiKey(oneKey[1], oneKey[0]));
        return apiKeys;
    }
}