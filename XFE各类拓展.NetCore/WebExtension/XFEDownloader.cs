using System.Diagnostics.CodeAnalysis;
using XFE各类拓展.NetCore.DelegateExtension;
using XFE各类拓展.NetCore.FileExtension;
using XFE各类拓展.NetCore.TaskExtension;

namespace XFE各类拓展.NetCore.WebExtension;

/// <summary>
/// XFE下载器
/// </summary>
[Obsolete("出现严重漏洞，暂停使用，预计下个版本恢复", true)]
public class XFEDownloader : IDisposable
{
    private bool disposedValue;
    private readonly HttpClient httpClient = new();
    private HttpResponseMessage? responseMessage;
    private readonly List<HttpResponseMessage> httpResponseMessages = [];
    private readonly List<Task> downloadTasks = [];
    /// <summary>
    /// 字节下载事件
    /// </summary>
    public event XFEEventHandler<XFEDownloader, FileDownloadedEventArgs>? BufferDownloaded;
    /// <summary>
    /// 目标下载地址
    /// </summary>
    public required string DownloadUrl { get; init; }
    /// <summary>
    /// 储存位置
    /// </summary>
    public required string SavePath { get; init; }
    /// <summary>
    /// 文件分段下载的数量（可加速下载）
    /// </summary>
    public int FileSegmentCount { get; init; } = 1;
    /// <summary>
    /// 已下载
    /// </summary>
    public bool Downloaded { get; private set; }
    /// <summary>
    /// 暂停
    /// </summary>
    public bool IsPaused { get; set; }
    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="continueFromLastDownload">是否为继续上次的下载</param>
    /// <returns></returns>
    public async Task Download(bool continueFromLastDownload = true)
    {
        var continueDownload = continueFromLastDownload && File.Exists(SavePath);
        long totalRead = 0;
        responseMessage ??= await GetHttpResponseMessage();
        long? totalFileSize = responseMessage.Content.Headers.ContentLength;
        using var createFileStream = new FileStream(SavePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 8192, true);
        if (totalFileSize is not null)
            createFileStream.SetLength(totalFileSize.Value);
        for (int i = 0; i < FileSegmentCount; i++)
        {
            int currentSegment = i;
            downloadTasks.Add(Task.Run(async () =>
            {
                using var fileStream = new FileStream(SavePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 8192, true);
                long endPosition = currentSegment == FileSegmentCount - 1 ? fileStream.Length - fileStream.Length / FileSegmentCount * currentSegment : fileStream.Length / FileSegmentCount * (currentSegment + 1);
                long startBufferIndex = fileStream.Length / FileSegmentCount * currentSegment;
                long lastBufferDownloadSize = continueDownload ? await fileStream.GetValidPosition(startBufferIndex, endPosition) : startBufferIndex;
                fileStream.Seek(lastBufferDownloadSize, SeekOrigin.Begin);
                long currentSegmentDownloadedBuffeSize = lastBufferDownloadSize - startBufferIndex;
                if (continueDownload)
                {
                    httpClient.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(lastBufferDownloadSize, endPosition);
                    totalRead += lastBufferDownloadSize;
                }
                using var contentStream = await (await GetHttpResponseMessage()).Content.ReadAsStreamAsync();
                byte[] buffer = new byte[8192];
                int currentRead;
                while ((currentRead = await contentStream.ReadAsync(buffer)) > 0 && !disposedValue)
                {
                    if (IsPaused)
                        while (!IsPaused && !disposedValue) { }
                    await fileStream.WriteAsync(buffer.AsMemory(0, currentRead));
                    totalRead += currentRead;
                    currentSegmentDownloadedBuffeSize += currentRead;
                    var bufferCopy = new byte[8192];
                    bufferCopy.CopyTo(buffer, 0);
                    if (totalRead == totalFileSize)
                        Downloaded = true;
                    BufferDownloaded?.Invoke(this, new FileDownloadedEventArgs(bufferCopy, totalRead, totalFileSize, fileStream.Length / FileSegmentCount, currentSegmentDownloadedBuffeSize, currentSegment, Downloaded));
                }
            }));
        }
        await Task.WhenAll(downloadTasks);
    }
    /// <summary>
    /// 暂停下载
    /// </summary>
    public void Pause() => IsPaused = true;
    /// <summary>
    /// 继续下载
    /// </summary>
    /// <returns></returns>
    public async Task Continue() => await Download();
    /// <summary>
    /// 获取下载信息
    /// </summary>
    /// <returns></returns>
    public async Task<(string? fileName, long? fileSize)> GetDownloadInfo()
    {
        responseMessage ??= await GetHttpResponseMessage();
        return (Path.GetFileName(responseMessage.RequestMessage?.RequestUri?.AbsolutePath), responseMessage.Content.Headers.ContentLength);
    }
    private async Task<HttpResponseMessage> GetHttpResponseMessage()
    {
        var response = await httpClient.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        return response;
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                httpClient.Dispose();
                responseMessage?.Dispose();
                foreach (var response in httpResponseMessages)
                {
                    response.Dispose();
                }
                foreach (var task in downloadTasks)
                {
                    task.Dispose();
                }
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            disposedValue = true;
        }
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    ~XFEDownloader()
    {
        Dispose(disposing: false);
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    /// <summary>
    /// XFE下载器
    /// </summary>
    public XFEDownloader() { }
    /// <summary>
    /// XFE下载器
    /// </summary>
    /// <param name="url">下载的URL地址</param>
    [SetsRequiredMembers]
    public XFEDownloader(string url)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException($"“{nameof(url)}”不能为 null 或空。", nameof(url));
        string filePath = url.GetFileNameFromURL().WaitAndGetResult()!;
        DownloadUrl = url;
        SavePath = filePath;
    }
    /// <summary>
    /// XFE下载器
    /// </summary>
    /// <param name="url">下载的URL地址</param>
    /// <param name="filePath">本地文件路径</param>
    [SetsRequiredMembers]
    public XFEDownloader(string url, string filePath)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException($"“{nameof(url)}”不能为 null 或空。", nameof(url));
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException($"“{nameof(filePath)}”不能为 null 或空。", nameof(filePath));
        DownloadUrl = url;
        SavePath = filePath;
    }
}
