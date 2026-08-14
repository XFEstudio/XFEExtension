namespace XFEExtension.NetCore.XFETransform.Json;

/// <summary>
/// JSON 属性名称的输出策略。
/// </summary>
public enum XFEJsonPropertyNamingPolicy
{
    /// <summary>
    /// 保持 CLR 成员名称不变。
    /// </summary>
    Unchanged,
    /// <summary>
    /// 将 CLR 成员名称的首字母转换为小写。
    /// </summary>
    CamelCase
}

/// <summary>
/// 反序列化遇到未映射成员时的处理方式。
/// </summary>
public enum XFEJsonUnmappedMemberHandling
{
    /// <summary>
    /// 忽略未映射成员。
    /// </summary>
    Ignore,
    /// <summary>
    /// 将未映射成员视为错误。
    /// </summary>
    Error
}

/// <summary>
/// 枚举的 JSON 输出格式。
/// </summary>
public enum XFEJsonEnumFormat
{
    /// <summary>
    /// 输出枚举的底层数值。
    /// </summary>
    Number,
    /// <summary>
    /// 输出枚举成员名称。
    /// </summary>
    String
}

/// <summary>
/// XFE JSON 编解码选项。实例在传入操作时会被复制，后续修改不会影响正在执行的操作。
/// </summary>
public sealed class XFEJsonOptions
{
    /// <summary>
    /// 默认选项。
    /// </summary>
    public static XFEJsonOptions Default { get; } = new();

    /// <summary>
    /// 最大 JSON 嵌套深度。
    /// </summary>
    public int MaxDepth { get; init; } = 128;

    /// <summary>
    /// POCO 成员绑定是否忽略属性名称大小写。
    /// </summary>
    public bool PropertyNameCaseInsensitive { get; init; } = true;

    /// <summary>
    /// CLR 成员名称的 JSON 输出策略。
    /// </summary>
    public XFEJsonPropertyNamingPolicy PropertyNamingPolicy { get; init; } = XFEJsonPropertyNamingPolicy.Unchanged;

    /// <summary>
    /// 是否包含公开字段。
    /// </summary>
    public bool IncludeFields { get; init; }

    /// <summary>
    /// 未映射 JSON 成员的处理方式。
    /// </summary>
    public XFEJsonUnmappedMemberHandling UnmappedMemberHandling { get; init; } = XFEJsonUnmappedMemberHandling.Ignore;

    /// <summary>
    /// 枚举的输出格式。
    /// </summary>
    public XFEJsonEnumFormat EnumFormat { get; init; } = XFEJsonEnumFormat.Number;

    /// <summary>
    /// 是否格式化 JSON 输出。
    /// </summary>
    public bool WriteIndented { get; init; }
}

internal readonly record struct JsonSettings(
    int MaxDepth,
    bool PropertyNameCaseInsensitive,
    XFEJsonPropertyNamingPolicy PropertyNamingPolicy,
    bool IncludeFields,
    XFEJsonUnmappedMemberHandling UnmappedMemberHandling,
    XFEJsonEnumFormat EnumFormat,
    bool WriteIndented)
{
    public static JsonSettings Create(XFEJsonOptions? options)
    {
        options ??= XFEJsonOptions.Default;
        if (options.MaxDepth is < 1 or > 2048)
            throw new ArgumentOutOfRangeException(nameof(options), options.MaxDepth, "MaxDepth 必须在 1 到 2048 之间。");

        return new(
            options.MaxDepth,
            options.PropertyNameCaseInsensitive,
            options.PropertyNamingPolicy,
            options.IncludeFields,
            options.UnmappedMemberHandling,
            options.EnumFormat,
            options.WriteIndented);
    }

    public XFEJsonOptions ToOptions() => new()
    {
        MaxDepth = MaxDepth,
        PropertyNameCaseInsensitive = PropertyNameCaseInsensitive,
        PropertyNamingPolicy = PropertyNamingPolicy,
        IncludeFields = IncludeFields,
        UnmappedMemberHandling = UnmappedMemberHandling,
        EnumFormat = EnumFormat,
        WriteIndented = WriteIndented
    };
}
