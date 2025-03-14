namespace XFEExtension.NetCore.FileExtension.XFEPackage;

/// <summary>
/// 打包文件
/// </summary>
public class XFEPackagedFile : XFEPackageBase
{
    /// <summary>
    /// 文件数据
    /// </summary>
    public byte[] Data { get; set; } = [];
}
