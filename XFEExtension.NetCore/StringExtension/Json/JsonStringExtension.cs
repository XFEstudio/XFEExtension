using XFEExtension.NetCore.XFETransform.Json;

namespace XFEExtension.NetCore.StringExtension.Json;

/// <summary>
/// 字符串查找及 JSON 编解码扩展。
/// </summary>
public static class JsonStringExtension
{
    /// <param name="str">被操作的字符串。</param>
    extension(string str)
    {
        /// <summary>
        /// 根据给定的开头和末尾返回查找到的第一个匹配字符串。
        /// </summary>
        public string GetStringBetweenTwoString(string beginString, string endString)
        {
            if (str == string.Empty)
                return string.Empty;
            var beginIndex = str.IndexOf(beginString, StringComparison.Ordinal);
            if (beginIndex is -1 or 0)
                return string.Empty;
            var endIndex = str.IndexOf(endString, beginIndex, StringComparison.Ordinal);
            return endIndex is -1 or 0
                ? string.Empty
                : str.Substring(beginIndex + beginString.Length, endIndex - beginIndex - beginString.Length);
        }

        /// <summary>
        /// 通过给定文本格式查找对应字段。
        /// </summary>
        public string GetTextByForm(string form) => str.GetStringBetweenTwoString(form + ":", ",");

        /// <summary>
        /// 将当前 JSON 字符串反序列化为指定类型。
        /// </summary>
        public T? FromJson<T>(XFEJsonOptions? options = null) => XFEJson.Deserialize<T>(str, options);
    }

    /// <param name="obj">待转换对象。</param>
    extension(object obj)
    {
        /// <summary>
        /// 转换为 JSON 字符串。
        /// </summary>
        public string ToJson(bool formatted = false) =>
            XFEJson.Serialize(obj, new XFEJsonOptions { WriteIndented = formatted });

        /// <summary>
        /// 使用指定选项转换为 JSON 字符串。
        /// </summary>
        public string ToJson(XFEJsonOptions options) => XFEJson.Serialize(obj, options);

        /// <summary>
        /// JSON 字符串文本。
        /// </summary>
        public string Json => obj.ToJson();
    }
}
