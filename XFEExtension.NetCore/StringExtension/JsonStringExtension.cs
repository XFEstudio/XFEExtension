namespace XFEExtension.NetCore.StringExtension.Json;

/// <summary>
/// 对string类进行Json操作的扩展
/// </summary>
public static class JsonStringExtension
{
    /// <summary>
    /// 根据给定的开头和末尾返回查找到的第一个匹配的字符串（全匹配）
    /// </summary>
    /// <param name="str">被匹配的字符串</param>
    /// <param name="beginString">匹配开头字符串</param>
    /// <param name="endString">匹配结尾字符串</param>
    /// <returns>返回夹在开头和末尾中间的字符串</returns>
    public static string GetStringBetweenTwoString(this string str, string beginString, string endString)
    {
        if (str != string.Empty && str is not null)
        {
            int beginIndex = str.IndexOf(beginString, StringComparison.Ordinal);
            if (beginIndex == -1 || beginIndex == 0)
            {
                return string.Empty;
            }
            int endIndex = str.IndexOf(endString, beginIndex, StringComparison.Ordinal);
            if (endIndex == -1 || endIndex == 0)
            {
                return string.Empty;
            }
            return str.Substring(beginIndex + beginString.Length, endIndex - beginIndex - beginString.Length);
        }
        else
        {
            return string.Empty;
        }
    }
    /// <summary>
    /// 通过给定的文本格式查找对应的字段
    /// </summary>
    /// <param name="str">待匹配文本</param>
    /// <param name="form">查找的文本格式</param>
    /// <returns></returns>
    public static string GetTextByForm(this string str, string form)
    {
        return GetStringBetweenTwoString(str, form + ":", ",");
    }
}