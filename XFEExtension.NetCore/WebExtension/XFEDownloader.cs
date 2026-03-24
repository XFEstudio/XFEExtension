using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using XFEExtension.NetCore.DelegateExtension;
using XFEExtension.NetCore.FileExtension;
using XFEExtension.NetCore.TaskExtension;

namespace XFEExtension.NetCore.WebExtension;

/// <summary>
/// XFE下载器
/// </summary>
public class XFEDownloader : IDisposable
{
    private bool _disposedValue;
    private readonly List<HttpClient> _httpClientList = [];
    private HttpResponseMessage? _responseMessage;
    private readonly List<Task> _downloadTasks = [];
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
        var getInfoClient = new HttpClient();
        _responseMessage ??= await GetHttpResponseMessage(getInfoClient, CancellationToken.None);
        var totalFileSize = _responseMessage.Content.Headers.ContentLength;
        await using var createFileStream = new FileStream(SavePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 8192, true);
        if (totalFileSize is not null)
            createFileStream.SetLength(totalFileSize.Value);
        for (var i = 0; i < FileSegmentCount; i++)
        {
            var currentSegment = i;
            _downloadTasks.Add(Task.Run(async () =>
            {
                await using var fileStream = new FileStream(SavePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 8192, true);
                var currentSegmentTotalBufferSize = fileStream.Length / FileSegmentCount;
                var endPosition = currentSegment == FileSegmentCount - 1 ? fileStream.Length : currentSegmentTotalBufferSize * (currentSegment + 1);
                var startBufferIndex = currentSegmentTotalBufferSize * currentSegment;
                var startPosition = continueDownload ? await fileStream.GetValidPosition(startBufferIndex, endPosition) : startBufferIndex;
                fileStream.Seek(startPosition, SeekOrigin.Begin);
                var currentSegmentDownloadedBuffeSize = startPosition - startBufferIndex;
                var httpClient = new HttpClient();
                _httpClientList.Add(httpClient);
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(startPosition, endPosition);
                if (continueDownload)
                    totalRead += currentSegmentDownloadedBuffeSize;
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                await using var contentStream = await (await GetHttpResponseMessage(httpClient, cancellationToken)).Content.ReadAsStreamAsync(cancellationToken);
                var buffer = new byte[8192];
                int currentRead;
                while ((currentRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0 && !_disposedValue)
                {
                    if (IsPaused)
                    {
                        await cancellationTokenSource.CancelAsync();
                        break;
                    }
                    await fileStream.WriteAsync(buffer.AsMemory(0, currentRead), cancellationToken);
                    totalRead += currentRead;
                    currentSegmentDownloadedBuffeSize += currentRead;
                    var bufferCopy = new byte[8192];
                    bufferCopy.CopyTo(buffer, 0);
                    if (Downloaded)
                        return;
                    if (totalRead >= totalFileSize)
                        Downloaded = true;
                    BufferDownloaded?.Invoke(this, new FileDownloadedEventArgs(bufferCopy, totalRead, totalFileSize, currentSegmentTotalBufferSize, currentSegmentDownloadedBuffeSize, currentSegment, Downloaded));
                }
            }));
        }
        await Task.WhenAll(_downloadTasks);
    }
    /// <summary>
    /// 暂停下载
    /// </summary>
    public void Pause() => IsPaused = true;
    /// <summary>
    /// 继续下载
    /// </summary>
    /// <returns></returns>
    public async Task Continue()
    {
        IsPaused = false;
        await Download();
    }

    /// <summary>
    /// 获取下载信息
    /// </summary>
    /// <returns></returns>
    public async Task<(string? fileName, long? fileSize)> GetDownloadInfo()
    {
        using var httpClient = new HttpClient();
        _responseMessage ??= await GetHttpResponseMessage(httpClient, CancellationToken.None);
        return (Path.GetFileName(_responseMessage.RequestMessage?.RequestUri?.AbsolutePath), _responseMessage.Content.Headers.ContentLength);
    }
    private async Task<HttpResponseMessage> GetHttpResponseMessage(HttpClient httpClient, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        return response;
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;
        if (disposing)
        {
            _responseMessage?.Dispose();
            foreach (var httpClient in _httpClientList)
            {
                httpClient.Dispose();
            }
            foreach (var task in _downloadTasks)
            {
                task.Dispose();
            }
        }

        // TODO: 释放未托管的资源(未托管的对象)并重写终结器
        // TODO: 将大型字段设置为 null
        _disposedValue = true;
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
        var filePath = url.GetFileNameFromUrl().WaitAndGetResult()!;
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
