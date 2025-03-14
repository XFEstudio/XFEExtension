namespace XFEExtension.NetCore.FileExtension.XFEPackage;

/// <summary>
/// 打包基类
/// </summary>
public abstract class XFEPackageBase
{
    /// <summary>
    /// 文件或目录名
    /// </summary>
    public string Name { get; set; } = string.Empty;
}