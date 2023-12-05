using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using XFE各类拓展.DelegateExtension;
using XFE各类拓展.TaskExtension;

namespace XFE各类拓展.WebExtension
{
    /// <summary>
    /// 网络拓展
    /// </summary>
    public static class WebExtension
    {
        /// <summary>
        /// 从URL获取字符串内容
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> GetFromURLAsync(this string url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 从URL获取字符串内容
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetFromURL(this string url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync(url).WaitAndGetResult();
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 从URL获取字符串内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content">POST的内容</param>
        /// <returns></returns>
        public static async Task<string> PostToURLAsync(this string url, string content)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(url, new StringContent(content));
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 从URL获取字符串内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content">POST的内容</param>
        /// <returns></returns>
        public static string PostToURL(this string url, string content)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = httpClient.PostAsync(url, new StringContent(content)).WaitAndGetResult();
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 从URL获取字符串内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content">POST的内容</param>
        /// <param name="contentType">POST的类型</param>
        /// <returns></returns>
        public static async Task<string> PostToURLAsync(this string url, string content, string contentType)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(url, new StringContent(content, Encoding.UTF8, contentType));
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 从URL获取字符串内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content">POST的内容</param>
        /// <param name="contentType">POST的类型</param>
        /// <returns></returns>
        public static string PostToURL(this string url, string content, string contentType)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = httpClient.PostAsync(url, new StringContent(content, Encoding.UTF8, contentType)).WaitAndGetResult();
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    return null;
                }
            }
        }
    }
    /// <summary>
    /// XFE下载器
    /// </summary>
    public class XFEDownloader
    {
        /// <summary>
        /// 字节下载事件
        /// </summary>
        public event XFEEventHandler<XFEDownloader, FileDownloadedEventArgs> BufferDownloaded;
        /// <summary>
        /// 目标下载地址
        /// </summary>
        public string DownloadUrl { get; set; }
        /// <summary>
        /// 储存位置
        /// </summary>
        public string SavePath { get; set; }
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
            using (var client = new HttpClient())
            {
                var continueDownload = continueFromLastDownload && File.Exists(SavePath);
                using (var fileStream = new FileStream(SavePath, continueDownload ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    long totalRead = 0;
                    long lastBufferDownloadSize = fileStream.Length;
                    if (continueDownload)
                    {
                        client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(lastBufferDownloadSize, null);
                        totalRead = lastBufferDownloadSize;
                    }
                    using (var response = await client.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        long? totalFileSize = response.Content.Headers.ContentLength + lastBufferDownloadSize;
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            byte[] buffer = new byte[8192];
                            int currentRead;
                            while ((currentRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, currentRead);
                                totalRead += currentRead;
                                var bufferCopy = new byte[8192];
                                bufferCopy.CopyTo(buffer, 0);
                                BufferDownloaded?.Invoke(this, new FileDownloadedEventArgs(bufferCopy, totalRead, totalFileSize, currentRead != 8192));
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 字符下载事件
    /// </summary>
    public class FileDownloadedEventArgs : EventArgs
    {
        /// <summary>
        /// 当前下载到的字节
        /// </summary>
        public byte[] CurrentBuffer { get; }
        /// <summary>
        /// 总计下载的字节长度
        /// </summary>
        public long DownloadedBufferSize { get; }
        /// <summary>
        /// 总共需要下载的字节大小
        /// </summary>
        public long? TotalBufferSize { get; }
        /// <summary>
        /// 是否下载完成
        /// </summary>
        public bool Downloaded { get; }
        /// <summary>
        /// 字符下载事件
        /// </summary>
        /// <param name="currentBuffer">当前下载的字符</param>
        /// <param name="downloadedBufferSize">已下载的字节大小</param>
        /// <param name="totalBufferSize">总计需要下载的字节大小</param>
        /// <param name="downloaded">是否已经下载完成</param>
        public FileDownloadedEventArgs(byte[] currentBuffer, long downloadedBufferSize, long? totalBufferSize, bool downloaded)
        {
            this.CurrentBuffer = currentBuffer;
            this.DownloadedBufferSize = downloadedBufferSize;
            this.TotalBufferSize = totalBufferSize;
            this.Downloaded = downloaded;
        }
    }
}