namespace XFE各类拓展.NetCore.FormatExtension;

/// <summary>
/// XFE可重复字典存储
/// </summary>
public class XFEMultiDictionary : ICollection<XFEEntry>
{
    private List<XFEEntry> xFEMultiDictionaryList = [];
    /// <summary>
    /// 字典中的条目数
    /// </summary>(T)item.Property
    public int Count => xFEMultiDictionaryList.Count;
    /// <summary>
    /// 只读
    /// </summary>
    public bool IsReadOnly => false;
    /// <summary>
    /// 从字符串中加载字典
    /// </summary>
    /// <param name="dictionaryString"></param>
    public void Load(string dictionaryString)
    {
        xFEMultiDictionaryList = ToList(dictionaryString);
    }
    /// <summary>
    /// 追加条目
    /// </summary>
    /// <param name="entry">条目对象</param>
    public void Add(XFEEntry entry)
    {
        xFEMultiDictionaryList.Add(entry);
    }
    /// <summary>
    /// 追加条目
    /// </summary>
    /// <param name="entryString">条目字符串</param>
    public void Add(string entryString)
    {
        var entry = XFEEntry.ToEntry(entryString);
        if (entry is not null)
            xFEMultiDictionaryList.Add(entry);
    }
    /// <summary>
    /// 追加条目
    /// </summary>
    /// <param name="header">头</param>
    /// <param name="content">内容</param>
    public void Add(string header, string content)
    {
        xFEMultiDictionaryList.Add(new XFEEntry(header, content));
    }
    /// <summary>
    /// 追加字典
    /// </summary>
    /// <param name="collection">字典对象</param>
    public void AddRange(List<XFEEntry> collection)
    {
        xFEMultiDictionaryList.AddRange(collection);
    }
    /// <summary>
    /// 追加字典
    /// </summary>
    /// <param name="dictionaryString">字典字符串</param>
    public void AddRange(string dictionaryString)
    {
        xFEMultiDictionaryList.AddRange(ToList(dictionaryString));
    }
    /// <summary>
    /// 插入条目
    /// </summary>
    /// <param name="index">插入位置</param>
    /// <param name="entry">条目</param>
    public void Insert(int index, XFEEntry entry)
    {
        xFEMultiDictionaryList.Insert(index, entry);
    }
    /// <summary>
    /// 插入条目
    /// </summary>
    /// <param name="index">插入位置</param>
    /// <param name="entryString">条目</param>
    public void Insert(int index, string entryString)
    {
        var entry = XFEEntry.ToEntry(entryString);
        if (entry is not null)
            xFEMultiDictionaryList.Insert(index, entry);
    }
    /// <summary>
    /// 插入条目
    /// </summary>
    /// <param name="index">插入位置</param>
    /// <param name="header">头</param>
    /// <param name="content">内容</param>
    public void Insert(int index, string header, string content)
    {
        xFEMultiDictionaryList.Insert(index, new XFEEntry(header, content));
    }
    /// <summary>
    /// 清空字典
    /// </summary>
    public void Clear()
    {
        xFEMultiDictionaryList.Clear();
    }
    /// <summary>
    /// 检测是否包含
    /// </summary>
    /// <param name="item">条目对象</param>
    /// <returns></returns>
    public bool Contains(XFEEntry item)
    {
        return xFEMultiDictionaryList.Contains(item);
    }
    /// <summary>
    /// 检测是否包含头
    /// </summary>
    /// <param name="headerString">指定头</param>
    /// <returns></returns>
    public bool Contains(string headerString)
    {
        return xFEMultiDictionaryList.Find(x => x.Header == headerString) is not null;
    }
    /// <summary>
    /// 检测是否包含内容
    /// </summary>
    /// <param name="contentString">指定内容</param>
    /// <returns></returns>
    public bool ContainContent(string contentString)
    {
        return xFEMultiDictionaryList.Find(x => x.Content == contentString) is not null;
    }
    /// <summary>
    /// 复制
    /// </summary>
    /// <param name="array">条目数组</param>
    /// <param name="arrayIndex">索引</param>
    public void CopyTo(XFEEntry[] array, int arrayIndex)
    {
        xFEMultiDictionaryList.CopyTo(array, arrayIndex);
    }
    /// <summary>
    /// 获取内容
    /// </summary>
    /// <param name="header">头</param>
    /// <returns></returns>
    public List<string> GetContents(string header)
    {
        var list = new List<string>();
        xFEMultiDictionaryList.FindAll(x => x.Header == header)?.ForEach(y => list.Add(y.Content));
        return list;
    }
    /// <summary>
    /// 获取索引器
    /// </summary>
    /// <returns></returns>
    public IEnumerator<XFEEntry> GetEnumerator()
    {
        return xFEMultiDictionaryList.GetEnumerator();
    }
    /// <summary>
    /// 移除指定的条目
    /// </summary>
    /// <param name="item">条目对象</param>
    /// <returns></returns>
    public bool Remove(XFEEntry item)
    {
        return xFEMultiDictionaryList.Remove(item);
    }
    /// <summary>
    /// 移除指定的条目
    /// </summary>
    /// <param name="headerString">头</param>
    /// <returns></returns>
    public bool Remove(string headerString)
    {
        var entry = xFEMultiDictionaryList.Find(x => x.Header == headerString);
        if (entry is not null)
            return xFEMultiDictionaryList.Remove(entry);
        else
            return false;
    }
    /// <summary>
    /// 移除指定范围的数组
    /// </summary>
    /// <param name="startIndex">起始位置</param>
    /// <param name="count">移除长度</param>
    public void RemoveRange(int startIndex, int count)
    {
        xFEMultiDictionaryList.RemoveRange(startIndex, count);
    }
    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="headerString">头</param>
    /// <returns></returns>
    public string? this[string headerString]
    {
        get
        {
            return xFEMultiDictionaryList.Find(x => x.Header == headerString)?.Content;
        }
        set
        {
            if (value is not null)
            {
                var entry = xFEMultiDictionaryList.Find(x => x.Header == headerString);
                if (entry is not null)
                    entry.Content = value;
            }
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return xFEMultiDictionaryList.GetEnumerator();
    }
    /// <summary>
    /// 转换为字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string result = "";
        foreach (var entry in xFEMultiDictionaryList)
        {
            result += entry.ToString();
        }
        return result;
    }
    /// <summary>
    /// 将字符串转换为XFEEntry的List
    /// </summary>
    /// <param name="dictionaryString">字符串</param>
    /// <returns></returns>
    protected static List<XFEEntry> ToList(string dictionaryString)
    {
        List<XFEEntry> xFEList = [];
        string[] messages = dictionaryString.Split(XFEDictionary.DictionarySeparator, StringSplitOptions.None);
        foreach (string message in messages)
        {
            string[] xFEDictionaryString = message.Split(XFEDictionary.EntrySeparator, StringSplitOptions.None);
            if (xFEDictionaryString.Length == 5)
            {
                xFEList.Add(new XFEEntry(xFEDictionaryString[1].Replace("[++", "[+").Replace("++]", "+]").Replace("|{++", "|{+").Replace("++}|", "+}|"), xFEDictionaryString[3].Replace("[++", "[+").Replace("++]", "+]").Replace("|{++", "|{+").Replace("++}|", "+}|")));
            }
        }
        return xFEList;
    }
    /// <summary>
    /// 将字符串转换为XFEMultiDictionary
    /// </summary>
    /// <param name="dictionaryString">字符串</param>
    /// <returns></returns>
    public static XFEMultiDictionary ToMultiDictionary(string dictionaryString)
    {
        return new XFEMultiDictionary(ToList(dictionaryString));
    }
    /// <summary>
    /// XFE字典存储
    /// </summary>
    /// <param name="xFEDictionaryList">条目列表</param>
    public XFEMultiDictionary(List<XFEEntry> xFEDictionaryList)
    {
        xFEMultiDictionaryList = xFEDictionaryList;
    }
    /// <summary>
    /// XFE字典存储
    /// </summary>
    /// <param name="headerAndContentStrings"></param>
    /// <exception cref="XFEDictionaryException"></exception>
    public XFEMultiDictionary(params string[] headerAndContentStrings)
    {
        if (!(headerAndContentStrings.Length % 2 == 0))
            throw new XFEDictionaryException("输入的字符串数组格式不正确，应为Header，Content，Header，Content...");
        for (int i = 0; i < headerAndContentStrings.Length; i += 2)
        {
            xFEMultiDictionaryList.Add(new XFEEntry(headerAndContentStrings[i], headerAndContentStrings[i + 1]));
        }
    }
    /// <summary>
    /// XFE字典存储
    /// </summary>
    /// <param name="dictionaryString">字典字符串</param>
    public XFEMultiDictionary(string dictionaryString)
    {
        xFEMultiDictionaryList = ToList(dictionaryString);
    }
    /// <summary>
    /// XFE字典存储
    /// </summary>
    public XFEMultiDictionary() { }
}