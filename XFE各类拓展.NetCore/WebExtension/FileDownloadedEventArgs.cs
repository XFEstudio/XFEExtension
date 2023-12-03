namespace XFE各类拓展.NetCore.WebExtension;

/// <summary>
/// 字符下载事件
/// </summary>
public class FileDownloadedEventArgs(byte[] currentBuffer, long downloadedBufferSize, long? totalBufferSize, bool downloaded) : EventArgs
{

    /// <summary>
    /// 当前下载到的字节
    /// </summary>
    public byte[] CurrentBuffer { get; } = currentBuffer;
    /// <summary>
    /// 总计下载的字节长度
    /// </summary>
    public long DownloadedBufferSize { get; } = downloadedBufferSize;
    /// <summary>
    /// 总共需要下载的字节大小
    /// </summary>
    public long? TotalBufferSize { get; } = totalBufferSize;
    /// <summary>
    /// 是否下载完成
    /// </summary>
    public bool Downloaded { get; } = downloaded;
}
