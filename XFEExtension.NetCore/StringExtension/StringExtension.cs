using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using XFEExtension.NetCore.XFETransform;
using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;
using XFEExtension.NetCore.XFETransform.StringConverter;

namespace XFEExtension.NetCore.StringExtension;

/// <summary>
/// 字符串拓展类
/// </summary>
public static partial class StringExtension
{
    #region 正则表达式
    [GeneratedRegex(@"^(\d{3,4}-)?\d{6,8}$")]
    private static partial Regex TelePhoneRegex();
    [GeneratedRegex(@"^1[3456789]\d{9}$")]
    private static partial Regex MobPhoneNumberRegex();
    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex PostalCodeRegex();
    [GeneratedRegex(@"^[0-9]*$")]
    private static partial Regex NumberRegex();
    [GeneratedRegex(@"(^\d{18}$)|(^\d{15}$)")]
    private static partial Regex IdCardRegex();
    [GeneratedRegex(@"(?:https?\:\/\/|[wW]{3}\.)(?:[a-zA-Z0-9\-]+(?:\.[a-zA-Z]{2,})+|localhost)(\:[0-9]{2,})?(?:\/[^\s]*)?")]
    private static partial Regex UrlRegex();
    #endregion

    /// <param name="telephoneNum"></param>
    extension(string telephoneNum)
    {
        /// <summary>
        /// 判断字符串是否为座机号码
        /// </summary>
        /// <returns></returns>
        public bool IsTelePhone() => TelePhoneRegex().IsMatch(telephoneNum);

        /// <summary>
        /// 判断字符串是否为手机号码
        /// </summary>
        /// <returns></returns>
        public bool IsMobPhoneNumber() => MobPhoneNumberRegex().IsMatch(telephoneNum);

        /// <summary>
        /// 判断字符串是否为邮政编码
        /// </summary>
        /// <returns></returns>
        public bool IsPostalCode() => PostalCodeRegex().IsMatch(telephoneNum);

        /// <summary>
        /// 判断字符串是否为数字
        /// </summary>
        /// <returns></returns>
        public bool IsNumber() => NumberRegex().IsMatch(telephoneNum);

        /// <summary>
        /// 判断字符串是否为身份证件号
        /// </summary>
        /// <returns></returns>
        public bool IsIdCard() => IdCardRegex().IsMatch(telephoneNum);

        /// <summary>
        /// 判断字符串是否为邮箱地址
        /// </summary>
        /// <returns></returns>
        public bool IsValidEmail()
        {
            if (string.IsNullOrWhiteSpace(telephoneNum))
                return false;

            try
            {
                telephoneNum = Regex.Replace(telephoneNum, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));
                _ = new MailAddress(telephoneNum).Host;
                return Regex.IsMatch(telephoneNum, """^(?(")(".+?"@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)[0-9a-zA-Z]\@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,}))$""", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// 验证是否是URL链接
        /// </summary>
        /// <returns></returns>
        public bool IsURL()
        {
            string pattern = @"^(https?|ftp|file|ws)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
            return IsMatch(pattern, telephoneNum);
        }

        /// <summary>
        /// 字符串实际显示的长度
        /// </summary>
        /// <returns></returns>
        public int DisplayLength()
        {
            int lengthCount = 0;
            var splits = telephoneNum.ToCharArray();
            foreach (var split in splits)
            {
                if (split == '\t')
                {
                    lengthCount += 8 - lengthCount % 8;
                }
                else
                {
                    if (split > 255)
                    {
                        lengthCount += 2;
                    }
                    else
                    {
                        lengthCount += 1;
                    }
                }
            }
            return lengthCount;
        }
    }

    /// <param name="str"></param>
    extension([NotNullWhen(false)] string? str)
    {
        /// <summary>
        /// 判断字符串是否是 null 、空字符串和仅包含空格的字符串
        /// </summary>
        public bool IsWhiteSpace { get=> str?.IsNullOrWhiteSpace() ?? true; }

        /// <summary>
        /// 判断字符串是否为 null 或者是空字符串
        /// </summary>
        /// <returns></returns>
        public bool IsNullOrEmpty() => string.IsNullOrEmpty(str);

        /// <summary>
        /// 判断字符串是否是 null 、空字符串和仅包含空格的字符串
        /// </summary>
        /// <returns></returns>
        public bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(str);
    }

    private static string DomainMapper(Match match)
    {
        var idn = new IdnMapping();
        var domainName = match.Groups[2].Value;
        domainName = idn.GetAscii(domainName);
        return match.Groups[1].Value + domainName;
    }

    /// <summary>
    /// 判断一个字符串，是否匹配指定的表达式(区分大小写的情况下)
    /// </summary>
    /// <param name="expression">正则表达式</param>
    /// <param name="str">要匹配的字符串</param>
    /// <returns></returns>
    public static bool IsMatch(string expression, string str) => !string.IsNullOrEmpty(str) && new Regex(expression).IsMatch(str);

    /// <param name="str"></param>
    extension(string? str)
    {
        /// <summary>
        /// 提取字符串中的URL链接
        /// </summary>
        /// <returns></returns>
        public string[]? GetUrl()
        {
            if (str is null)
                return null;
            Regex regex = UrlRegex();
            MatchCollection matches = regex.Matches(str);
            string[] result = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
                result[i] = matches[i].Value;
            return result;
        }

        /// <summary>
        /// 替换字符串中的多个字符串为指定字符串
        /// </summary>
        /// <param name="replaceStringArray">替换的字符串组</param>
        /// <param name="tarString">目的字符串</param>
        /// <returns></returns>
        public string? ReplaceStrings(string[] replaceStringArray, string tarString) => replaceStringArray.Aggregate(str, (current, replaceString) => current?.Replace(replaceString, tarString));

        /// <summary>
        /// 匹配两个字符串之间的字符串
        /// </summary>
        /// <param name="startString">起始字符串</param>
        /// <param name="endString">结束字符串</param>
        /// <returns></returns>
        public string[] MatchStringsBetween(string startString, string endString)
        {
            if (str.IsNullOrWhiteSpace())
                return [];
            string pattern = $"{Regex.Escape(startString)}(.*?){Regex.Escape(endString)}";
            var matches = Regex.Matches(str, pattern);
            string[] result = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
                result[i] = matches[i].Groups[1].Value;
            return result;
        }

        /// <summary>
        /// 文本自动换行
        /// </summary>
        /// <param name="width">一行文本的最大长度</param>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(str))]
        public string? WrapText(int width)
        {
            if (str is null)
                return str;
            StringBuilder stringBuilder = new();
            int startIndex = 0;
            while (startIndex < str.Length)
            {
                // 寻找下一个换行位置
                int endIndex = startIndex + width;
                if (endIndex >= str.Length)
                {
                    // 如果已经到达文本末尾，则直接添加剩余部分并结束循环
                    stringBuilder.Append(str[startIndex..]);
                    break;
                }

                // 在指定宽度范围内寻找最后一个空格字符
                int lastSpaceIndex = str.LastIndexOf(' ', endIndex, width);
                if (lastSpaceIndex > startIndex)
                {
                    // 如果找到了空格字符，则将文本从起始位置到空格字符位置添加到结果中，并进行换行
                    stringBuilder.Append(str[startIndex..lastSpaceIndex]);
                    stringBuilder.AppendLine();
                    startIndex = lastSpaceIndex + 1;
                }
                else
                {
                    // 如果没有找到空格字符，则直接添加指定宽度的文本到结果中，并进行换行
                    stringBuilder.Append(str.AsSpan(startIndex, width));
                    stringBuilder.AppendLine();
                    startIndex += width;
                }
            }

            return stringBuilder.ToString();
        }
    }

    /// <param name="obj"></param>
    extension(object obj)
    {
        /// <summary>
        /// 输出到控制台并返回
        /// </summary>
        /// <returns>ToString后的内容</returns>
        public string? WriteLineToConsole()
        {
            Console.WriteLine(obj);
            return obj.ToString();
        }

        /// <summary>
        /// 输出到控制台
        /// </summary>
        public void CW()
        {
            Console.WriteLine(obj);
        }
    }

    /// <param name="obj">待分析的对象</param>
    extension(object? obj)
    {
        /// <summary>
        /// 分析对象并输出到控制台
        /// </summary>
        /// <param name="remarkName">对象别名</param>
        /// <param name="onlyProperty"></param>
        /// <param name="onlyPublic"></param>
        public string? X(bool onlyProperty = true, bool onlyPublic = true, string remarkName = "分析对象")
        {
            var fatherList = new List<object>();
            if (obj is not null)
                fatherList.Add(obj);
            var result = XFEConverter.GetObjectInfo(StringConverter.ObjectAnalyzer, remarkName, ObjectPlace.Main, 0, fatherList, obj?.GetType(), obj, onlyProperty, onlyPublic).OutPutObject();
            Console.WriteLine(result);
            return result;
        }

        /// <summary>
        /// 分析对象并输出到跟踪输出
        /// </summary>
        /// <param name="remarkName">对象别名</param>
        /// <param name="onlyProperty"></param>
        /// <param name="onlyPublic"></param>
        public string XL(string remarkName = "分析对象", bool onlyProperty = true, bool onlyPublic = true)
        {
            var fatherList = new List<object>();
            if (obj is not null)
                fatherList.Add(obj);
            var result = XFEConverter.GetObjectInfo(StringConverter.ObjectAnalyzer, remarkName, ObjectPlace.Main, 0, fatherList, obj?.GetType(), obj, onlyProperty, onlyPublic).OutPutObject();
            Trace.WriteLine(result);
            return result;
        }
    }

    /// <summary>
    /// 随机生成指定长度的字符串
    /// </summary>
    /// <param name="length">字符串的长度</param>
    /// <returns></returns>
    public static string GenerateRandomString(int length)
    {
        string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new();
        char[] randomChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            int randomIndex = random.Next(0, allowedChars.Length);
            randomChars[i] = allowedChars[randomIndex];
        }

        return new string(randomChars);
    }
    /// <summary>
    /// 随机生成指定长度指定字符串组合的字符串
    /// </summary>
    /// <param name="length">字符串的长度</param>
    /// <param name="allowedChars">允许的字符串</param>
    /// <returns></returns>
    public static string GenerateRandomString(int length, string allowedChars)
    {
        Random random = new();
        char[] randomChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            int randomIndex = random.Next(0, allowedChars.Length);
            randomChars[i] = allowedChars[randomIndex];
        }

        return new string(randomChars);
    }
    #region 返回包含分割器的字符串

    /// <param name="str"></param>
    extension(string str)
    {
        /// <summary>
        /// 返回包含分割器的字符串
        /// </summary>
        /// <param name="delimiter">分割器组</param>
        /// <param name="removeEmptyString">是否移除空字符串</param>
        /// <returns>分割后的带分割器字符串数组</returns>
        public string[] SplitAndKeepDelimiter(string[] delimiter, bool removeEmptyString)
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
        /// <param name="delimiter">分割器</param>
        /// <param name="removeEmptyString">是否移除空字符串</param>
        /// <returns>分割后的带分割器字符串数组</returns>
        public string[] SplitAndKeepDelimiter(string delimiter, bool removeEmptyString)
        {
            return str.SplitAndKeepDelimiter([delimiter], removeEmptyString);
        }

        /// <summary>
        /// 返回包含分割器的字符串
        /// </summary>
        /// <param name="delimiter">分割器</param>
        /// <returns>分割后的带分割器字符串数组</returns>
        public string[] SplitAndKeepDelimiter(string delimiter)
        {
            return str.SplitAndKeepDelimiter([delimiter], true);
        }

        /// <summary>
        /// 返回包含分割器的字符串
        /// </summary>
        /// <param name="delimiter">分割器组</param>
        /// <returns>分割后的带分割器字符串数组</returns>
        public string[] SplitAndKeepDelimiter(string[] delimiter)
        {
            return str.SplitAndKeepDelimiter(delimiter, true);
        }
    }

    #endregion
}