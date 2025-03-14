using XFEExtension.NetCore.BufferExtension;

namespace XFEExtension.NetCore.FileExtension.XFEPackage;

/// <summary>
/// XFE文件打包协议
/// </summary>
public static class XFEPackage
{
    /// <summary>
    /// 打包文件
    /// </summary>
    /// <param name="packageFilePath">待打包的文件夹路径</param>
    /// <param name="fileName">打包后的文件路径</param>
    public static void PackFile(string packageFilePath, string fileName) => File.WriteAllBytes(fileName, PackFile(packageFilePath));

    /// <summary>
    /// 打包文件
    /// </summary>
    /// <param name="packageFilePath">待打包的文件夹路径</param>
    /// <returns>打包后的文件</returns>
    public static byte[] PackFile(string packageFilePath)
    {
        var buffer = new XFEBuffer();
        var packagedDirectory = GetFilePackage(packageFilePath);
        buffer.Add(Path.GetFileName(packageFilePath), packagedDirectory.Package());
        return buffer.ToBuffer();
    }

    /// <summary>
    /// 解包文件
    /// </summary>
    /// <param name="packageFilePath">待解包文件路径</param>
    /// <param name="outputDirectory">输出文件夹</param>
    public static void UnPackFile(string packageFilePath, string outputDirectory) => UnPackFile(packageFilePath).UnPackage(outputDirectory);

    /// <summary>
    /// 解包文件
    /// </summary>
    /// <param name="packageFilePath">待解包文件路径</param>
    /// <returns>解包后的文件</returns>
    public static XFEPackagedDirectory UnPackFile(string packageFilePath) => GetFilePackage(File.ReadAllBytes(packageFilePath));

    internal static XFEPackagedDirectory GetFilePackage(string packageFilePath)
    {
        if (!Directory.Exists(packageFilePath))
        {
            throw new DirectoryNotFoundException();
        }
        var packagedDirectory = new XFEPackagedDirectory
        {
            Name = Path.GetFileName(packageFilePath)
        };
        foreach (var file in Directory.GetFiles(packageFilePath))
        {
            var packagedFile = new XFEPackagedFile
            {
                Name = Path.GetFileName(file),
                Data = File.ReadAllBytes(file)
            };
            packagedDirectory.PackagedFiles.Add(packagedFile);
        }
        foreach (var directory in Directory.GetDirectories(packageFilePath))
            packagedDirectory.PackagedDirectories.Add(GetFilePackage(directory));
        return packagedDirectory;
    }

    internal static XFEPackagedDirectory GetFilePackage(byte[] filePackageBytes)
    {
        var buffer = XFEBuffer.ToXFEBuffer(filePackageBytes);
        if (buffer.FirstOrDefault() is KeyValuePair<string, byte[]> keyValuePair)
            return GetFilePackageRecursion(keyValuePair.Key, keyValuePair.Value);
        else
            return new XFEPackagedDirectory();
    }

    internal static XFEPackagedDirectory GetFilePackageRecursion(string packageName, byte[] filePackageBytes)
    {
        var packagedDirectory = new XFEPackagedDirectory
        {
            Name = packageName
        };
        var buffer = XFEBuffer.ToXFEBuffer(filePackageBytes);
        foreach (var (name, data) in buffer)
        {
            if (name.StartsWith("D|"))
            {
                var packagedSubDirectory = GetFilePackageRecursion(name, data);
                packagedSubDirectory.Name = name[2..];
                packagedDirectory.PackagedDirectories.Add(packagedSubDirectory);
            }
            else
            {
                var packagedFile = new XFEPackagedFile
                {
                    Name = name,
                    Data = data
                };
                packagedDirectory.PackagedFiles.Add(packagedFile);
            }
        }
        return packagedDirectory;
    }
}