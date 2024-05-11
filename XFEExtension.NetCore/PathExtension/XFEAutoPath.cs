namespace XFEExtension.NetCore.PathExtension;

/// <summary>
/// 自动目录
/// </summary>
public static class XFEAutoPath
{
    /// <summary>
    /// 检测文件夹路径是否存在否则创建
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <param name="enableCheck">是否启用检测</param>
    public static void CheckPathExistAndCreate(string path, bool enableCheck = true)
    {
        if (enableCheck && !Path.Exists(path))
            Directory.CreateDirectory(path);
    }
}