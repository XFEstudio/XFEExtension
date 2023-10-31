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
}
