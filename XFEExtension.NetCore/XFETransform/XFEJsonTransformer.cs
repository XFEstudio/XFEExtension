using XFEExtension.NetCore.Exceptions;
using XFEExtension.NetCore.XFETransform.Json;

namespace XFEExtension.NetCore.XFETransform;

/// <summary>
/// 旧版 JSON 转换入口。新代码应使用 <see cref="XFEJson"/>。
/// </summary>
public static class XFEJsonTransformer
{
    /// <summary>
    /// 将对象转换为 JSON 字符串。
    /// </summary>
    [Obsolete("请使用 XFEJson.Serialize 或 ToJson。")]
    public static string? ConvertToJson(this object? obj)
    {
        if (obj is null)
            throw new XFEJsonTransformException("对象为空");
        return XFEJson.Serialize(obj);
    }

    /// <summary>
    /// 将 JSON 字符串转换为对象。
    /// </summary>
    [Obsolete("请使用 XFEJson.Deserialize<T> 或 FromJson<T>。")]
    public static T ConvertFromJson<T>(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
            throw new ArgumentException("JSON 字符串为空。", nameof(jsonString));
        return XFEJson.Deserialize<T>(jsonString)!;
    }
}
