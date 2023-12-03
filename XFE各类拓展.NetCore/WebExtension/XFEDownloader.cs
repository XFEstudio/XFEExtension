using XFE各类拓展.NetCore.DelegateExtension;

namespace XFE各类拓展.NetCore.WebExtension;

/// <summary>
/// XFE下载器
/// </summary>
public class XFEDownloader
{
    /// <summary>
    /// 字节下载事件
    /// </summary>
    public event XFEEventHandler<XFEDownloader, FileDownloadedEventArgs>? BufferDownloaded;
    /// <summary>
    /// 目标下载地址
    /// </summary>
    public required string DownloadUrl { get; set; }
    /// <summary>
    /// 储存位置
    /// </summary>
    public required string SavePath { get; set; }
    /// <summary>
    /// 已下载
    /// </summary>
    public bool Downloaded { get; }
    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="continueFromLastDownload"></param>
    /// <returns></returns>
    public async Task Download(bool continueFromLastDownload = true)
    {
        using var client = new HttpClient();
        var continueDownload = continueFromLastDownload && File.Exists(SavePath);
        using var fileStream = new FileStream(SavePath, continueDownload ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
        long totalRead = 0;
        long lastBufferDownloadSize = fileStream.Length;
        if (continueDownload)
        {
            client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(lastBufferDownloadSize, null);
            totalRead = lastBufferDownloadSize;
        }
        using var response = await client.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        long? totalFileSize = response.Content.Headers.ContentLength + lastBufferDownloadSize;
        using var contentStream = await response.Content.ReadAsStreamAsync();
        byte[] buffer = new byte[8192];
        int currentRead;
        while ((currentRead = await contentStream.ReadAsync(buffer)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, currentRead));
            totalRead += currentRead;
            var bufferCopy = new byte[8192];
            bufferCopy.CopyTo(buffer, 0);
            BufferDownloaded?.Invoke(this, new FileDownloadedEventArgs(bufferCopy, totalRead, totalFileSize, currentRead != 8192));
        }
    }
}
