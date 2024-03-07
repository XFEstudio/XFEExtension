using XFEExtension.NetCore.TaskExtension;

namespace XFEExtension.NetCore.FileExtension;

/// <summary>
/// XFE文件监视器
/// </summary>
public class XFEFileWatcher
{
    #region 字段
    private readonly List<FileSystemWatcher> watchers = [];
    #endregion
    #region 属性
    /// <summary>
    /// 是否遍历监视所有子文件夹
    /// </summary>
    public bool WatchSubdirectories { get; set; }
    /// <summary>
    /// 文件或文件夹路径
    /// </summary>
    public string? Path { get; set; }
    #endregion
    #region 事件
    /// <summary>
    /// 文件发生改变时触发
    /// </summary>
    public event FileSystemEventHandler? FileChanged;
    /// <summary>
    /// 文件被创建时触发
    /// </summary>
    public event FileSystemEventHandler? FileCreated;
    /// <summary>
    /// 文件被删除时触发
    /// </summary>
    public event FileSystemEventHandler? FileDeleted;
    /// <summary>
    /// 文件被重命名时触发
    /// </summary>
    public event RenamedEventHandler? FileRenamed;
    #endregion
    #region 方法
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Changed:
                FileChanged?.Invoke(sender, e);
                break;
            case WatcherChangeTypes.Created:
                FileCreated?.Invoke(sender, e);
                break;
            case WatcherChangeTypes.Deleted:
                FileDeleted?.Invoke(sender, e);
                break;
            default:
                break;
        }
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        FileRenamed?.Invoke(sender, e);
    }

    private void MonitorSubdirectories(string folderPath)
    {
        string[] subdirectories = Directory.GetDirectories(folderPath);
        foreach (string subdirectory in subdirectories)
        {
            try
            {
                FileSystemWatcher subdirectoryWatcher = new(subdirectory)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
                };
                watchers.Add(subdirectoryWatcher);
                subdirectoryWatcher.Changed += OnFileChanged;
                subdirectoryWatcher.Created += OnFileChanged;
                subdirectoryWatcher.Deleted += OnFileChanged;
                subdirectoryWatcher.Renamed += OnFileRenamed;
                subdirectoryWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                continue;
            }
            MonitorSubdirectories(subdirectory);
        }
    }
    /// <summary>
    /// 启动监视
    /// </summary>
    /// <returns></returns>
    /// <exception cref="XFEExtensionException"></exception>
    public async Task StartWatchingAsync()
    {
        if (Path is not null && Path != string.Empty)
        {
            if (File.Exists(Path) || Directory.Exists(Path))
            {
                if (WatchSubdirectories && Directory.Exists(Path))
                {
                    await new Action(() => { MonitorSubdirectories(Path); }).StartNewTask();
                }
                FileSystemWatcher rootWatcher = new(Path);
                watchers.Add(rootWatcher);
                rootWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                rootWatcher.Changed += OnFileChanged;
                rootWatcher.Created += OnFileChanged;
                rootWatcher.Deleted += OnFileChanged;
                rootWatcher.Renamed += OnFileRenamed;
            }
            else
            {
                throw new XFEExtensionException("文件或文件夹不存在");
            }
        }
        else
        {
            throw new XFEExtensionException("未设置属性");
        }
    }
    /// <summary>
    /// 启动监视
    /// </summary>
    /// <returns></returns>
    /// <exception cref="XFEExtensionException"></exception>
    public void StartWatching()
    {
        if (Path is not null && Path != string.Empty)
        {
            FileSystemWatcher rootWatcher;
            if (Directory.Exists(Path))
            {
                if (WatchSubdirectories)
                {
                    MonitorSubdirectories(Path);
                }
                rootWatcher = new(Path);

            }
            else if (File.Exists(Path))
            {
                rootWatcher = new(System.IO.Path.GetDirectoryName(Path)!)
                {
                    Filter = System.IO.Path.GetFileName(Path)
                };
            }
            else
            {
                throw new XFEExtensionException("文件或文件夹不存在");
            }
            watchers.Add(rootWatcher);
            rootWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            rootWatcher.Changed += OnFileChanged;
            rootWatcher.Created += OnFileChanged;
            rootWatcher.Deleted += OnFileChanged;
            rootWatcher.Renamed += OnFileRenamed;
            rootWatcher.EnableRaisingEvents = true;
        }
        else
        {
            throw new XFEExtensionException("未设置属性");
        }
    }
    /// <summary>
    /// 停止监视
    /// </summary>
    public void StopWatching()
    {
        foreach (FileSystemWatcher watcher in watchers)
        {
            watcher.EnableRaisingEvents = false;
        }
    }
    /// <summary>
    /// 继续监视
    /// </summary>
    public void ContinueWatching()
    {
        foreach (FileSystemWatcher watcher in watchers)
        {
            watcher.EnableRaisingEvents = true;
        }
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        foreach (FileSystemWatcher watcher in watchers)
        {
            watcher.Dispose();
        }
    }
    #endregion
    #region 构造函数
    /// <summary>
    /// XFE文件监视器
    /// </summary>
    public XFEFileWatcher() { }
    /// <summary>
    /// XFE文件监视器
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="watchSubdirectories">是否遍历监视所有子文件/文件夹</param>
    public XFEFileWatcher(string path, bool watchSubdirectories = false)
    {
        Path = path;
        WatchSubdirectories = watchSubdirectories;
    }
    #endregion
}
