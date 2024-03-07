namespace XFEExtension.NetCore.WebExtension;

/// <summary>
/// 字符下载事件
/// </summary>
public class FileDownloadedEventArgs(byte[] currentBuffer, long downloadedBufferSize, long? totalBufferSize, long currentSegmentTotalBufferSize, long currentSegmentDownloadedBufferSize, int currentSegmentIndex, bool downloaded) : EventArgs
{

    /// <summary>
    /// 当前下载到的字节
    /// </summary>
    public byte[] CurrentBuffer { get; } = currentBuffer;
    /// <summary>
    /// 总共已下载的字节大小
    /// </summary>
    public long DownloadedBufferSize { get; } = downloadedBufferSize;
    /// <summary>
    /// 总共需要下载的字节大小
    /// </summary>
    public long? TotalBufferSize { get; } = totalBufferSize;
    /// <summary>
    /// 当前节的总计需要下载的字节大小
    /// </summary>
    public long CurrentSegmentTotalBufferSize { get; } = currentSegmentTotalBufferSize;
    /// <summary>
    /// 当前节的已下载的字节大小
    /// </summary>
    public long CurrentSegmentDownloadedBufferSize { get; } = currentSegmentDownloadedBufferSize;
    /// <summary>
    /// 当前节的索引
    /// </summary>
    public int CurrentSegmentIndex { get; } = currentSegmentIndex;
    /// <summary>
    /// 是否下载完成
    /// </summary>
    public bool Downloaded { get; } = downloaded;
}