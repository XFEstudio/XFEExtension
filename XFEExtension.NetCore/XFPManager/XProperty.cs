namespace XFEExtension.NetCore.XFPManager;

internal class XProperty
{
    /// <summary>
    /// 获取或设置由属性管理器读取或写入的属性值。
    /// </summary>
    public object? Property { get; set; }

    /// <summary>
    /// 获取或设置该属性在被管理对象中的名称。
    /// </summary>
    public string? Name { get; set; }
}
