using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace XFE各类拓展.NetCore.FileExtension;

/// <summary>
/// 文件的拓展
/// </summary>
public static class FileExtension
{
    /// <summary>
    /// 将字符串写入文件
    /// </summary>
    /// <param name="text"></param>
    /// <param name="fileName">目标文件路径及文件名</param>
    public static void WriteIn(this string text, string fileName)
    {
        FileInfo fileInfo = new(fileName);
        using var fileStream = fileInfo.Create();
        var stream = Encoding.UTF8.GetBytes(text);
        fileStream.Write(stream, 0, stream.Length);
        fileStream.Flush();
    }
    /// <summary>
    /// 将非法字符串写入文件
    /// </summary>
    /// <param name="objectToWrite"></param>
    /// <param name="fileName">文件名</param>
    [Obsolete("序列化方法在.NET 8.0中已经不再受支持")]
    public static void WriteInObj(this object objectToWrite, string fileName)
    {
        FileInfo fileInfo = new(fileName);
        using var fileStream = fileInfo.Create();
        BinaryFormatter binaryFormatter = new();
        binaryFormatter.Serialize(fileStream, objectToWrite);
        fileStream.Flush();
        fileStream.Close();
    }
    /// <summary>
    /// 读取文件中的字符串
    /// </summary>
    /// <param name="name"></param>
    /// <param name="content">读取的内容</param>
    /// <returns>被读取的文件是否存在</returns>
    public static bool ReadOut(this string name, out string? content)
    {
        FileInfo fileInfo = new(name);
        if (fileInfo.Exists)
        {
            using StreamReader streamReader = fileInfo.OpenText();
            content = streamReader.ReadLine();
            streamReader.Close();
            return true;
        }
        else
        {
            content = string.Empty;
            return false;
        }
    }
    /// <summary>
    /// 读取文件中的字符串
    /// </summary>
    /// <param name="fileName">目标文件路径及文件名</param>
    /// <returns>目标文件内容，如不存在则返回-1</returns>
    public static string? ReadOut(this string fileName)
    {
        FileInfo fileInfo = new(fileName);
        if (fileInfo.Exists)
        {
            using var streamReader = fileInfo.OpenText();
            var text = streamReader.ReadLine();
            streamReader.Close();
            return text;
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
    [Obsolete("序列化方法在.NET 8.0中已经不再受支持")]
    public static T? ReadOutObj<T>(this string fileName)
    {
        FileInfo fileInfo = new(fileName);
        if (fileInfo.Exists)
        {
            using var fileStream = fileInfo.OpenRead();
            BinaryFormatter bf = new();
            var result = (T)bf.Deserialize(fileStream);
            fileStream.Close();
            return result;
        }
        else
        {
            return default;
        }
    }
    /// <summary>
    /// 读取文件中的字符串
    /// </summary>
    /// <param name="fileName">目标文件路径及文件名</param>
    /// <param name="exist">目标文件是否存在</param>
    /// <returns>目标文件内容，如不存在则返回-1</returns>
    public static string? ReadOut(string fileName, out bool exist)
    {
        FileInfo fileInfo = new(fileName);
        exist = fileInfo.Exists;
        if (fileInfo.Exists)
        {
            using StreamReader streamReader = fileInfo.OpenText();
            var text = streamReader.ReadLine();
            streamReader.Close();
            return text;
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
    [Obsolete("序列化方法在.NET 8.0中已经不再受支持")]
    public static string? ReadOutObj(this string fileName, out bool exist)
    {
        FileInfo fileInfo = new(fileName);
        exist = fileInfo.Exists;
        if (fileInfo.Exists)
        {
            using var fileStream = fileInfo.OpenRead();
            BinaryFormatter binaryFormatter = new();
            var text = binaryFormatter.Deserialize(fileStream).ToString();
            fileStream.Close();
            return text;
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
    [Obsolete("序列化方法在.NET 8.0中已经不再受支持")]
    public static void SerializeToFile(this object obj, string fileName)
    {
        BinaryFormatter writeBinary = new();
        using var WriteStream = File.OpenWrite(fileName);
        writeBinary.Serialize(WriteStream, obj);
        WriteStream.Flush();
        WriteStream.Close();
    }
    /// <summary>
    /// 从文件反序列化对象
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [Obsolete("序列化方法在.NET 8.0中已经不再受支持")]
    public static T DeserializeFromFile<T>(this string fileName)
    {
        BinaryFormatter readBinary = new();
        using var readStream = File.OpenRead(fileName);
        T obj = (T)readBinary.Deserialize(readStream);
        readStream.Close();
        return obj;
    }
    /// <summary>
    /// 输出文件大小
    /// </summary>
    /// <param name="bufferLength"></param>
    /// <returns></returns>
    public static string FileSize(this long bufferLength)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        double len = bufferLength;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return string.Format("{0:0.##} {1}", len, sizes[order]);
    }
    /// <summary>
    /// 获取占用某个文件的程序
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns></returns>
    public static Process? GetOwningProcess(this string filePath)
    {
        try
        {
            // 获取所有正在运行的进程
            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                try
                {
                    // 获取进程打开的文件句柄信息
                    foreach (ProcessModule module in process.Modules)
                    {
                        if (module.FileName.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                        {
                            return process; // 返回占用文件的进程
                        }
                    }
                }
                catch (Exception)
                {
                    // 忽略无法访问的进程
                }
            }
        }
        catch { }
        return null; // 如果没有找到占用文件的进程，返回null
    }
}
