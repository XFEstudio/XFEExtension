using XFEExtension.NetCore.XFETransform;
using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;
using XFEExtension.NetCore.XFETransform.StringConverter;

namespace XFEExtension.NetCore.StringExtension.Json;

/// <summary>
/// 对string类进行Json操作的扩展
/// </summary>
public static class JsonStringExtension
{
    /// <param name="str">被匹配的字符串</param>
    extension(string str)
    {
        /// <summary>
        /// 根据给定的开头和末尾返回查找到的第一个匹配的字符串（全匹配）
        /// </summary>
        /// <param name="beginString">匹配开头字符串</param>
        /// <param name="endString">匹配结尾字符串</param>
        /// <returns>返回夹在开头和末尾中间的字符串</returns>
        public string GetStringBetweenTwoString(string beginString, string endString)
        {
            if (str == string.Empty)
                return string.Empty;
            var beginIndex = str.IndexOf(beginString, StringComparison.Ordinal);
            if (beginIndex is -1 or 0)
                return string.Empty;
            var endIndex = str.IndexOf(endString, beginIndex, StringComparison.Ordinal);
            return endIndex is -1 or 0 ? string.Empty : str.Substring(beginIndex + beginString.Length, endIndex - beginIndex - beginString.Length);
        }

        /// <summary>
        /// 通过给定的文本格式查找对应的字段
        /// </summary>
        /// <param name="form">查找的文本格式</param>
        /// <returns></returns>
        public string GetTextByForm(string form)
        {
            return str.GetStringBetweenTwoString(form + ":", ",");
        }
    }

    /// <param name="obj">待转换对象</param>
    extension(object obj)
    {
        /// <summary>
        /// 转化为Json字符串文本
        /// </summary>
        /// <param name="formatted">格式化Json（自动换行和空格对齐）</param>
        /// <returns></returns>
        public string ToJson(bool formatted = false) => XFEConverter.GetObjectInfo(formatted ? StringConverter.FormattedJsonTransformer : StringConverter.JsonTransformer, string.Empty, ObjectPlace.Main, 0, [obj], obj.GetType(), obj).OutPutObject();

        /// <summary>
        /// 转化为Json字符串文本
        /// </summary>
        public string Json { get => obj.ToJson(); }
    }
}