using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
    public class XFEDownloader
    {
        public event EventHandler<FileDownloadedEventArgs> BufferDownloaded;
        public string DownloadUrl { get; set; }
        public string SavePath { get; set; }
        public async Task Download()
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    long? totalFileSize = response.Content.Headers.ContentLength;

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        using (FileStream fileStream = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            byte[] buffer = new byte[8192];
                            long totalRead = 0;
                            int read;

                            while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                totalRead += read;

                                if (totalFileSize.HasValue)
                                {
                                    double percentage = ((double)totalRead / totalFileSize.Value) * 100;
                                    Console.WriteLine($"Downloaded {totalRead}/{totalFileSize} bytes ({percentage:F2}%)");
                                }
                                else
                                {
                                    Console.WriteLine($"Downloaded {totalRead} bytes");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public class FileDownloadedEventArgs : EventArgs
    {
        public long CurrentBufferSize { get; set; }
        public long DownloadedBufferSize { get; set; }
        public long TotalBufferSize { get; set; }
    }
}
