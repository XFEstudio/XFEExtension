using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using XFE各类拓展.TaskExtension;

namespace XFE各类拓展.FileExtension
{
    /// <summary>
    /// 文件的拓展
    /// </summary>
    public static class FileExtension
    {
        /// <summary>
        /// 将字符串写入文件
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="fileName">目标文件路径及文件名</param>
        public static void WriteIn(this string txt, string fileName)
        {
            FileInfo rd = new FileInfo(fileName);
            using (FileStream fs = rd.Create())
            {
                var stream = Encoding.UTF8.GetBytes(txt);
                fs.Write(stream, 0, stream.Length);
                fs.Flush();
            }
        }
        /// <summary>
        /// 将非法字符串写入文件
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="fileName">文件名</param>
        public static void WriteInObj(this object bin, string fileName)
        {
            FileInfo rd = new FileInfo(fileName);
            using (FileStream fs = rd.Create())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, bin);
                fs.Flush();
                fs.Close();
            }
        }
        /// <summary>
        /// 读取文件中的字符串
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Content">读取的内容</param>
        /// <returns>被读取的文件是否存在</returns>
        public static bool ReadOut(this string Name, out string Content)
        {
            FileInfo rd = new FileInfo(Name);
            if (rd.Exists)
            {
                using (StreamReader sr = rd.OpenText())
                {
                    Content = sr.ReadLine();
                    sr.Close();
                    return true;
                }
            }
            else
            {
                Content = string.Empty;
                return false;
            }
        }
        /// <summary>
        /// 读取文件中的字符串
        /// </summary>
        /// <param name="fileName">目标文件路径及文件名</param>
        /// <returns>目标文件内容，如不存在则返回-1</returns>
        public static string ReadOut(this string fileName)
        {
            FileInfo rd = new FileInfo(fileName);
            if (rd.Exists)
            {
                using (StreamReader sr = rd.OpenText())
                {
                    string text = sr.ReadLine();
                    sr.Close();
                    return text;
                }
            }
            else
            {
                return "-1";
            }
        }
        /// <summary>
        /// 读取文件中的字符串
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T ReadOutObj<T>(this string fileName)
        {
            FileInfo rd = new FileInfo(fileName);
            if (rd.Exists)
            {
                using (FileStream fs = rd.OpenRead())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    var result = (T)bf.Deserialize(fs);
                    fs.Close();
                    return result;
                }
            }
            else
            {
                return default(T);
            }
        }
        /// <summary>
        /// 读取文件中的字符串
        /// </summary>
        /// <param name="fileName">目标文件路径及文件名</param>
        /// <param name="exist">目标文件是否存在</param>
        /// <returns>目标文件内容，如不存在则返回-1</returns>
        public static string ReadOut(string fileName, out bool exist)
        {
            FileInfo rd = new FileInfo(fileName);
            exist = rd.Exists;
            if (rd.Exists)
            {
                using (StreamReader sr = rd.OpenText())
                {
                    string text = sr.ReadLine();
                    sr.Close();
                    return text;
                }
            }
            else
            {
                return "-1";
            }
        }
        /// <summary>
        /// 读取文件中的字符串
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="exist">是否存在</param>
        /// <returns></returns>
        public static string ReadOutObj(this string fileName, out bool exist)
        {
            FileInfo rd = new FileInfo(fileName);
            exist = rd.Exists;
            if (rd.Exists)
            {
                using (FileStream fs = rd.OpenRead())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    string bin = bf.Deserialize(fs).ToString();
                    fs.Close();
                    return bin;
                }
            }
            else
            {
                return "-1";
            }
        }
        /// <summary>
        /// 序列化对象到文件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fileName">文件路径</param>
        public static void SerializeToFile(this object obj, string fileName)
        {
            BinaryFormatter WriteBinary = new BinaryFormatter();
            using (var WriteStream = File.OpenWrite(fileName))
            {
                WriteBinary.Serialize(WriteStream, obj);
                WriteStream.Flush();
                WriteStream.Close();
            }
        }
        /// <summary>
        /// 从文件反序列化对象
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(this string fileName)
        {
            BinaryFormatter ReadBinary = new BinaryFormatter();
            using (var ReadStream = File.OpenRead(fileName))
            {
                T obj = (T)ReadBinary.Deserialize(ReadStream);
                ReadStream.Close();
                return obj;
            }
        }
    }
    /// <summary>
    /// XFE文件监视器
    /// </summary>
    public class XFEFileWatcher
    {
        #region 字段
        private List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
        #endregion
        #region 属性
        /// <summary>
        /// 是否遍历监视所有子文件夹
        /// </summary>
        public bool WatchSubdirectories { get; set; }
        /// <summary>
        /// 文件或文件夹路径
        /// </summary>
        public string Path { get; set; }
        #endregion
        #region 事件
        /// <summary>
        /// 文件发生改变时触发
        /// </summary>
        public event FileSystemEventHandler FileChanged;
        /// <summary>
        /// 文件被创建时触发
        /// </summary>
        public event FileSystemEventHandler FileCreated;
        /// <summary>
        /// 文件被删除时触发
        /// </summary>
        public event FileSystemEventHandler FileDeleted;
        /// <summary>
        /// 文件被重命名时触发
        /// </summary>
        public event RenamedEventHandler FileRenamed;
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
                    FileSystemWatcher subdirectoryWatcher = new FileSystemWatcher(subdirectory);
                    subdirectoryWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
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
            if (Path != null && Path != string.Empty)
            {
                if (File.Exists(Path) || Directory.Exists(Path))
                {
                    if (WatchSubdirectories && Directory.Exists(Path))
                    {
                        await new Action(() => { MonitorSubdirectories(Path); }).StartNewTask();
                    }
                    FileSystemWatcher rootWatcher = new FileSystemWatcher(Path);
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
            if (Path != null && Path != string.Empty)
            {
                if (File.Exists(Path) || Directory.Exists(Path))
                {
                    if (WatchSubdirectories && Directory.Exists(Path))
                    {
                        MonitorSubdirectories(Path);
                    }
                    FileSystemWatcher rootWatcher = new FileSystemWatcher(Path);
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
}
