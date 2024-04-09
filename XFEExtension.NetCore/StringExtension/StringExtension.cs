using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using XFEExtension.NetCore.XFETransform;

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
    [GeneratedRegex(@"(?:https?|www)\:\/\/[a-zA-Z0-9\-]+(?:\.[a-zA-Z]{2,})+(?:\/[^\s]*)?")]
    private static partial Regex UrlRegex();
    #endregion
    /// <summary>
    /// 判断字符串是否为座机号码
    /// </summary>
    /// <param name="telephoneNum"></param>
    /// <returns></returns>
    public static bool IsTelePhone(this string telephoneNum)
    {
        return TelePhoneRegex().IsMatch(telephoneNum);
    }
    /// <summary>
    /// 判断字符串是否为手机号码
    /// </summary>
    /// <param name="mobPhoneNum"></param>
    /// <returns></returns>
    public static bool IsMobPhoneNumber(this string mobPhoneNum)
    {
        return MobPhoneNumberRegex().IsMatch(mobPhoneNum);
    }
    /// <summary>
    /// 判断字符串是否为邮政编码
    /// </summary>
    /// <param name="postalCode"></param>
    /// <returns></returns>
    public static bool IsPostalCode(this string postalCode)
    {
        return PostalCodeRegex().IsMatch(postalCode);
    }
    /// <summary>
    /// 判断字符串是否为数字
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsNumber(this string number)
    {
        return NumberRegex().IsMatch(number);
    }
    /// <summary>
    /// 判断字符串是否为身份证件号
    /// </summary>
    /// <param name="idNumber"></param>
    /// <returns></returns>
    public static bool IsIdCard(this string idNumber)
    {
        return IdCardRegex().IsMatch(idNumber);
    }

    /// <summary>
    /// 判断字符串是否为 null 或者是空字符串
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

    /// <summary>
    /// 判断字符串是否是 null 、空字符串和仅包含空格的字符串
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

    /// <summary>
    /// 判断字符串是否为邮箱地址
    /// </summary>
    /// <param name="emailString"></param>
    /// <returns></returns>
    public static bool IsValidEmail(this string emailString)
    {
        if (string.IsNullOrWhiteSpace(emailString))
            return false;

        try
        {
            emailString = Regex.Replace(emailString, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));
            var validDomain = new System.Net.Mail.MailAddress(emailString).Host;
            return Regex.IsMatch(emailString, @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)[0-9a-zA-Z]\@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,}))$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (FormatException)
        {
            return false;
        }
    }
    private static string DomainMapper(Match match)
    {
        var idn = new IdnMapping();
        var domainName = match.Groups[2].Value;
        domainName = idn.GetAscii(domainName);
        return match.Groups[1].Value + domainName;
    }
    /// <summary>
    /// 验证是否是URL链接
    /// </summary>
    /// <param name="str">指定字符串</param>
    /// <returns></returns>
    public static bool IsURL(this string str)
    {
        string pattern = @"^(https?|ftp|file|ws)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
        return IsMatch(pattern, str);
    }
    /// <summary>
    /// 判断一个字符串，是否匹配指定的表达式(区分大小写的情况下)
    /// </summary>
    /// <param name="expression">正则表达式</param>
    /// <param name="str">要匹配的字符串</param>
    /// <returns></returns>
    public static bool IsMatch(string expression, string str)
    {
        Regex reg = new(expression);
        if (string.IsNullOrEmpty(str))
            return false;
        return reg.IsMatch(str);
    }
    /// <summary>
    /// 提取字符串中的URL链接
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string[]? GetUrl(this string str)
    {
        if (str is not null)
        {
            Regex regex = UrlRegex();
            MatchCollection matches = regex.Matches(str);
            string[] result = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                result[i] = matches[i].Value;
            }
            return result;
        }
        else
        {
            return null;
        }
    }
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
        StringBuilder stringBuilder = new();
        int startIndex = 0;
        while (startIndex < text.Length)
        {
            // 寻找下一个换行位置
            int endIndex = startIndex + width;
            if (endIndex >= text.Length)
            {
                // 如果已经到达文本末尾，则直接添加剩余部分并结束循环
                stringBuilder.Append(text[startIndex..]);
                break;
            }
            else
            {
                // 在指定宽度范围内寻找最后一个空格字符
                int lastSpaceIndex = text.LastIndexOf(' ', endIndex, width);
                if (lastSpaceIndex > startIndex)
                {
                    // 如果找到了空格字符，则将文本从起始位置到空格字符位置添加到结果中，并进行换行
                    stringBuilder.Append(text[startIndex..lastSpaceIndex]);
                    stringBuilder.AppendLine();
                    startIndex = lastSpaceIndex + 1;
                }
                else
                {
                    // 如果没有找到空格字符，则直接添加指定宽度的文本到结果中，并进行换行
                    stringBuilder.Append(text.AsSpan(startIndex, width));
                    stringBuilder.AppendLine();
                    startIndex += width;
                }
            }
        }

        return stringBuilder.ToString();
    }
    /// <summary>
    /// 输出到控制台并返回
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>ToString后的内容</returns>
    public static string? WriteLineToConsole(this object obj)
    {
        Console.WriteLine(obj);
        return obj.ToString();
    }
    /// <summary>
    /// 输出到控制台
    /// </summary>
    /// <param name="obj"></param>
    public static void CW(this object obj)
    {
        Console.WriteLine(obj);
    }
    /// <summary>
    /// 分析对象并输出到控制台
    /// </summary>
    /// <param name="obj">待分析的对象</param>
    /// <param name="remarkName">对象别名</param>
    /// <param name="onlyProperty"></param>
    /// <param name="onlyPublic"></param>
    public static void X(this object? obj, string remarkName = "分析对象", bool onlyProperty = true, bool onlyPublic = true) => Console.WriteLine(XFEConverter.GetObjectInfo(new ObjectAnalyzer(), remarkName, ObjectPlace.Main, 0, obj?.GetType(), obj, onlyProperty, onlyPublic).OutPutObject());
    /// <summary>
    /// 分析对象并输出到跟踪输出
    /// </summary>
    /// <param name="obj">待分析的对象</param>
    /// <param name="remarkName">对象别名</param>
    /// <param name="onlyProperty"></param>
    /// <param name="onlyPublic"></param>
    public static void XL(this object? obj, string remarkName = "分析对象", bool onlyProperty = true, bool onlyPublic = true) => Trace.WriteLine(XFEConverter.GetObjectInfo(new ObjectAnalyzer(), remarkName, ObjectPlace.Main, 0, obj?.GetType(), obj, onlyProperty, onlyPublic).OutPutObject());
    /// <summary>
    /// 字符串实际显示的长度
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int DisplayLength(this string str)
    {
        int lengthCount = 0;
        var splits = str.ToCharArray();
        for (int i = 0; i < splits.Length; i++)
        {
            if (splits[i] == '\t')
            {
                lengthCount += 8 - lengthCount % 8;
            }
            else
            {
                if (splits[i] > 255)
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