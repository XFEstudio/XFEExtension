using System.Text;
using XFE各类拓展.NetCore.TaskExtension;

namespace XFE各类拓展.NetCore.WebExtension;

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
    public static async Task<string?> GetFromURLAsync(this string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException($"“{nameof(url)}”不能为 null 或空。", nameof(url));
        }

        using var httpClient = new HttpClient();
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
    /// <summary>
    /// 从URL获取字符串内容
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static string? GetFromURL(this string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException($"“{nameof(url)}”不能为 null 或空。", nameof(url));
        }

        using var httpClient = new HttpClient();
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
    /// <summary>
    /// 从URL获取字符串内容
    /// </summary>
    /// <param name="url"></param>
    /// <param name="content">POST的内容</param>
    /// <returns></returns>
    public static async Task<string?> PostToURLAsync(this string url, string content)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException($"“{nameof(url)}”不能为 null 或空。", nameof(url));
        }

        using var httpClient = new HttpClient();
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
    /// <summary>
    /// 从URL获取字符串内容
    /// </summary>
    /// <param name="url"></param>
    /// <param name="content">POST的内容</param>
    /// <returns></returns>
    public static string? PostToURL(this string url, string content)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException($"“{nameof(url)}”不能为 null 或空。", nameof(url));
        }

        using var httpClient = new HttpClient();
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
    /// <summary>
    /// 从URL获取字符串内容
    /// </summary>
    /// <param name="url"></param>
    /// <param name="content">POST的内容</param>
    /// <param name="contentType">POST的类型</param>
    /// <returns></returns>
    public static async Task<string?> PostToURLAsync(this string url, string content, string contentType)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException($"“{nameof(url)}”不能为 null 或空。", nameof(url));
        }

        using var httpClient = new HttpClient();
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
    /// <summary>
    /// 从URL获取字符串内容
    /// </summary>
    /// <param name="url"></param>
    /// <param name="content">POST的内容</param>
    /// <param name="contentType">POST的类型</param>
    /// <returns></returns>
    public static string? PostToURL(this string url, string content, string contentType)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException($"“{nameof(url)}”不能为 null 或空。", nameof(url));
        }

        using var httpClient = new HttpClient();
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