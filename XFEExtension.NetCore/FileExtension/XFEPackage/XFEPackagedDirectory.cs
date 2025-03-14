using XFEExtension.NetCore.BufferExtension;

namespace XFEExtension.NetCore.FileExtension.XFEPackage;

/// <summary>
/// 打包目录
/// </summary>
public class XFEPackagedDirectory : XFEPackageBase
{
    /// <summary>
    /// 打包的文件
    /// </summary>
    public List<XFEPackagedFile> PackagedFiles { get; set; } = [];
    /// <summary>
    /// 打包的目录
    /// </summary>
    public List<XFEPackagedDirectory> PackagedDirectories { get; set; } = [];

    /// <summary>
    /// 打包
    /// </summary>
    /// <returns></returns>
    public byte[] Package()
    {
        var buffer = new XFEBuffer();
        foreach (var packagedFile in PackagedFiles)
            buffer.Add(packagedFile.Name, packagedFile.Data);
        foreach (var packagedDirectory in PackagedDirectories)
            buffer.Add($"D|{packagedDirectory.Name}", packagedDirectory.Package());
        return buffer.ToBuffer();
    }

    /// <summary>
    /// 解包
    /// </summary>
    /// <param name="path">指定目录</param>
    public void UnPackage(string path)
    {
        Directory.CreateDirectory(path);
        foreach (var packagedFile in PackagedFiles)
            File.WriteAllBytes(Path.Combine(path, packagedFile.Name), packagedFile.Data);
        foreach (var packagedDirectory in PackagedDirectories)
            packagedDirectory.UnPackage(Path.Combine(path, packagedDirectory.Name));
    }
}
