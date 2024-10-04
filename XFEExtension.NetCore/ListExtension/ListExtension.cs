namespace XFEExtension.NetCore.ListExtension;

/// <summary>
/// 列表的拓展
/// </summary>
public static class ListExtension
{
    /// <summary>
    /// 将List列表转换为XFE格式字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string ToXFEString<T>(this List<T> list)
    {
        string str = string.Empty;
        foreach (var ary in list)
        {
            if (ary is not null)
                str += $"[+-{ary.ToString()?.Replace("[+", "[++").Replace("+]", "++]")}-+]";
        }
        return str;
    }
    /// <summary>
    /// 将List列表转换为XFE格式字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="propertyName">属性名称</param>
    /// <returns></returns>
    public static string ToXFEString<T>(this List<T> list, string propertyName) where T : class
    {
        string str = string.Empty;
        foreach (var ary in list)
        {
            if (ary is not null)
                str += $"[+-{ary.GetType().GetProperty(propertyName)?.GetValue(ary)?.ToString()?.Replace("[+", "[++").Replace("+]", "++]")}-+]";
        }
        return str;
    }
    /// <summary>
    /// 将XFE格式字符串转换为List列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="str"></param>
    /// <returns>T类型的List列表</returns>
    public static List<T> ToXFEList<T>(this string str)
    {
        string[] strings = str.Split(new string[] { "[+-", "-+]" }, StringSplitOptions.RemoveEmptyEntries);
        List<T> list = new List<T>();
        for (int j = 1; j < strings.Length - 1; j += 2)
        {
            list.Add((T)Convert.ChangeType(strings[j].Replace("[++", "[+").Replace("++]", "+]"), typeof(T)));
        }
        return list;
    }
}
