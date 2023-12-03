namespace XFE各类拓展.NetCore.FormatExtension;

/// <summary>
/// XFE条目
/// </summary>
/// <param name="Header">头</param>
/// <param name="Content">内容</param>
public class XFEEntry(string Header, string Content)
{
    /// <summary>
    /// 头
    /// </summary>
    public string Header { get; set; } = Header;
    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = Content;
    /// <summary>
    /// 转换为字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"|{{+-[+-{Header.Replace("[+", "[++").Replace("+]", "++]").Replace("|{+", "|{++").Replace("+}|", "++}|")}-+][+-{Content.Replace("[+", "[++").Replace("+]", "++]").Replace("|{+", "|{++").Replace("+}|", "++}|")}-+]-+}}|";
    }
    /// <summary>
    /// 将字符串转化为条目对象
    /// </summary>
    /// <param name="EntryString"></param>
    /// <returns></returns>
    public static XFEEntry? ToEntry(string EntryString)
    {
        string[] messages = EntryString.Split(XFEDictionary.DictionarySeparator, StringSplitOptions.None);
        foreach (string message in messages)
        {
            string[] xFEDictionaryString = message.Split(XFEDictionary.EntrySeparator, StringSplitOptions.None);
            if (xFEDictionaryString.Length == 5)
            {
                return new XFEEntry(xFEDictionaryString[1].Replace("[++", "[+").Replace("++]", "+]").Replace("|{++", "|{+").Replace("++}|", "+}|"), xFEDictionaryString[3].Replace("[++", "[+").Replace("++]", "+]").Replace("|{++", "|{+").Replace("++}|", "+}|"));
            }
        }
        return null;
    }
}