namespace XFEExtension.NetCore.XFETransform.Json;

/// <summary>
/// XFE JSON 的惰性解析、验证、序列化和反序列化入口。
/// </summary>
public static class XFEJson
{
    /// <summary>
    /// 创建惰性 JSON 根节点。此操作不会验证或扫描完整 JSON。
    /// </summary>
    public static XFEJsonNode Parse(string json, XFEJsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        var source = new JsonSource(json, JsonSettings.Create(options));
        return new XFEJsonNode(source, 0, -1, XFEJsonValueKind.Undefined, "$", 0, isRoot: true);
    }

    /// <summary>
    /// 验证完整 JSON 文本。
    /// </summary>
    public static void Validate(string json, XFEJsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        JsonScanner.ValidateDocument(new JsonSource(json, JsonSettings.Create(options)));
    }

    /// <summary>
    /// 尝试验证完整 JSON 文本。
    /// </summary>
    public static bool TryValidate(string json, out XFEJsonError? error, XFEJsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            Validate(json, options);
            error = null;
            return true;
        }
        catch (XFEJsonException exception)
        {
            error = exception.Error;
            return false;
        }
    }

    /// <summary>
    /// 将值序列化为 JSON 文本。
    /// </summary>
    public static string Serialize<T>(T? value, XFEJsonOptions? options = null) =>
        JsonCodec.Serialize(value, typeof(T), JsonSettings.Create(options));

    /// <summary>
    /// 将 JSON 文本反序列化为指定类型。
    /// </summary>
    public static T? Deserialize<T>(string json, XFEJsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        var node = Parse(json, options);
        var result = node.Deserialize<T>();
        node.Validate();
        return result;
    }

    /// <summary>
    /// 将 JSON 文本反序列化为运行时指定的类型。
    /// </summary>
    public static object? Deserialize(string json, Type targetType, XFEJsonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(targetType);
        var settings = JsonSettings.Create(options);
        var node = Parse(json, options);
        var result = JsonCodec.Deserialize(node, targetType, settings);
        node.Validate();
        return result;
    }
}
