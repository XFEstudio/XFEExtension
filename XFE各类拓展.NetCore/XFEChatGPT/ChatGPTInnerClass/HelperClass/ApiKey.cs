using XFE各类拓展.NetCore.FormatExtension;

namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

/// <summary>
/// API密钥
/// </summary>
/// <param name="aPIKey">秘钥</param>
/// <param name="description">描述</param>
public class ApiKey(string aPIKey, string description)
{
    private static readonly char[] separator = ['{', '}'];
    /// <summary>
    /// APIKEY
    /// </summary>
    public string APIKey { get; set; } = aPIKey;
    /// <summary>
    /// APIKey的描述
    /// </summary>
    public string Description { get; set; } = description;
    /// <summary>
    /// 转换为字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{{[+-{Description}-+][+-{APIKey}-+]}}";
    }
    /// <summary>
    /// 将字符串转换为ApiKey的List
    /// </summary>
    /// <param name="keyString"></param>
    /// <returns></returns>
    public static List<ApiKey> ToApiKey(string keyString)
    {
        string[] keyGroup = keyString.Split(separator);
        List<ApiKey> apiKeys = [];
        foreach (string key in keyGroup)
        {
            string[] oneKey = key.Split(XFEDictionary.EntrySeparator, StringSplitOptions.RemoveEmptyEntries);
            if (oneKey.Length == 2)
            {
                apiKeys.Add(new ApiKey(oneKey[1], oneKey[0]));
            }
        }
        return apiKeys;
    }
}