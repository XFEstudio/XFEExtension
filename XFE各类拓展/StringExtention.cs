using System;
using System.Text;
using System.Text.RegularExpressions;

namespace XFE各类拓展.StringExtension
{
    namespace Json
    {
        /// <summary>
        /// 对string类进行Json操作的扩展
        /// </summary>
        public static class JsonStringExtension
        {
            /// <summary>
            /// 根据给定的开头和末尾返回查找到的第一个匹配的字符串（全匹配）
            /// </summary>
            /// <param name="str">被匹配的字符串</param>
            /// <param name="beginstr">匹配开头字符串</param>
            /// <param name="endstr">匹配结尾字符串</param>
            /// <returns>返回夹在开头和末尾中间的字符串</returns>
            public static string GetStringBetweenTwoString(this string str, string beginstr, string endstr)
            {
                if (str != string.Empty && str != null)
                {
                    int beginindex = str.IndexOf(beginstr, StringComparison.Ordinal);
                    if (beginindex == -1 || beginindex == 0)
                    {
                        return string.Empty;
                    }
                    int endindex = str.IndexOf(endstr, beginindex, StringComparison.Ordinal);
                    if (endindex == -1 || endindex == 0)
                    {
                        return string.Empty;
                    }
                    return str.Substring(beginindex + beginstr.Length, endindex - beginindex - beginstr.Length);
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
    }
    /// <summary>
    /// 字符串拓展类
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 替换字符串中的多个字符串为指定字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="replaceStringArray">替换的字符串组</param>
        /// <param name="tarString">目的字符串</param>
        /// <returns></returns>
        public static string ReplaceStrings(this string str, string[] replaceStringArray, string tarString)
        {
            foreach (string replaceString in replaceStringArray)
            {
                str = str.Replace(replaceString, tarString);
            }
            return str;
        }
        /// <summary>
        /// 匹配两个字符串之间的字符串
        /// </summary>
        /// <param name="input"></param>
        /// <param name="startString">起始字符串</param>
        /// <param name="endString">结束字符串</param>
        /// <returns></returns>
        public static string[] MatchStringsBetween(this string input, string startString, string endString)
        {
            string pattern = $"{Regex.Escape(startString)}(.*?){Regex.Escape(endString)}";
            MatchCollection matches = Regex.Matches(input, pattern);

            string[] result = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                result[i] = matches[i].Groups[1].Value;
            }

            return result;
        }
        /// <summary>
        /// 文本自动换行
        /// </summary>
        /// <param name="text"></param>
        /// <param name="width">一行文本的最大长度</param>
        /// <returns></returns>
        public static string WrapText(this string text, int width)
        {
            StringBuilder sb = new StringBuilder();
            int startIndex = 0;
            while (startIndex < text.Length)
            {
                // 寻找下一个换行位置
                int endIndex = startIndex + width;
                if (endIndex >= text.Length)
                {
                    // 如果已经到达文本末尾，则直接添加剩余部分并结束循环
                    sb.Append(text.Substring(startIndex));
                    break;
                }
                else
                {
                    // 在指定宽度范围内寻找最后一个空格字符
                    int lastSpaceIndex = text.LastIndexOf(' ', endIndex, width);
                    if (lastSpaceIndex > startIndex)
                    {
                        // 如果找到了空格字符，则将文本从起始位置到空格字符位置添加到结果中，并进行换行
                        sb.Append(text.Substring(startIndex, lastSpaceIndex - startIndex));
                        sb.AppendLine();
                        startIndex = lastSpaceIndex + 1;
                    }
                    else
                    {
                        // 如果没有找到空格字符，则直接添加指定宽度的文本到结果中，并进行换行
                        sb.Append(text.Substring(startIndex, width));
                        sb.AppendLine();
                        startIndex += width;
                    }
                }
            }

            return sb.ToString();
        }
        /// <summary>
        /// 输出到控制台并返回
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>ToString后的内容</returns>
        public static string ConsoleWriteLine(this object obj)
        {
            Console.WriteLine(obj);
            return obj.ToString();
        }
        #region 返回包含分割器的字符串
        /// <summary>
        /// 返回包含分割器的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="delimiter">分割器组</param>
        /// <param name="removeEmptyString">是否移除空字符串</param>
        /// <returns>分割后的带分割器字符串数组</returns>
        public static string[] SplitAndKeepDelimiter(this string str, string[] delimiter, bool removeEmptyString)
        {
            string[] result;
            if (removeEmptyString)
            {
                result = str.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                result = str.Split(delimiter, StringSplitOptions.None);
            }
            for (int i = 0; i < result.Length - 1; i++)
            {
                result[i] += delimiter;
            }
            return result;
        }
        /// <summary>
        /// 返回包含分割器的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="delimiter">分割器</param>
        /// <param name="removeEmptyString">是否移除空字符串</param>
        /// <returns>分割后的带分割器字符串数组</returns>
        public static string[] SplitAndKeepDelimiter(this string str, string delimiter, bool removeEmptyString)
        {
            return SplitAndKeepDelimiter(str, new string[] { delimiter }, removeEmptyString);
        }
        /// <summary>
        /// 返回包含分割器的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="delimiter">分割器</param>
        /// <returns>分割后的带分割器字符串数组</returns>
        public static string[] SplitAndKeepDelimiter(this string str, string delimiter)
        {
            return SplitAndKeepDelimiter(str, new string[] { delimiter }, true);
        }
        /// <summary>
        /// 返回包含分割器的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="delimiter">分割器组</param>
        /// <returns>分割后的带分割器字符串数组</returns>
        public static string[] SplitAndKeepDelimiter(this string str, string[] delimiter)
        {
            return SplitAndKeepDelimiter(str, delimiter, true);
        }
        #endregion
    }
}