namespace XFE各类拓展.NetCore.FormatExtension;

/// <summary>
/// XFE唯一字典存储
/// </summary>
public class XFEDictionary : ICollection<XFEEntry>
{
    /// <summary>
    /// 字典分隔符
    /// </summary>
    public static string[] DictionarySeparator { get; } = ["|{+-", "-+}|"];
    /// <summary>
    /// 条目分隔符
    /// </summary>
    public static string[] EntrySeparator { get; } = ["[+-", "-+]"];
    private List<XFEEntry> xFEDictionaryList = [];
    /// <summary>
    /// 字典中的条目数
    /// </summary>
    public int Count => xFEDictionaryList.Count;
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
        xFEDictionaryList = ToList(dictionaryString);
    }
    /// <summary>
    /// 追加条目
    /// </summary>
    /// <param name="entry">条目对象</param>
    /// <exception cref="XFEDictionaryException"></exception>
    public void Add(XFEEntry entry)
    {
        if (Contains(entry.Header))
            throw new XFEDictionaryException("Header的值应唯一！");
        xFEDictionaryList.Add(entry);
    }
    /// <summary>
    /// 追加条目
    /// </summary>
    /// <param name="entryString">条目字符串</param>
    /// <exception cref="XFEDictionaryException"></exception>
    public void Add(string entryString)
    {
        var entry = XFEEntry.ToEntry(entryString);
        if (entry is not null)
        {
            if (Contains(entry.Header))
                throw new XFEDictionaryException("Header的值应唯一！");
            xFEDictionaryList.Add(entry);
        }
    }
    /// <summary>
    /// 追加条目
    /// </summary>
    /// <param name="header">头</param>
    /// <param name="content">内容</param>
    /// <exception cref="XFEDictionaryException"></exception>
    public void Add(string header, string content)
    {
        if (Contains(header))
            throw new XFEDictionaryException("Header的值应唯一！");
        xFEDictionaryList.Add(new XFEEntry(header, content));
    }
    /// <summary>
    /// 追加字典
    /// </summary>
    /// <param name="collection">字典对象</param>
    /// <exception cref="XFEDictionaryException"></exception>
    public void AddRange(List<XFEEntry> collection)
    {
        var duplicates = collection.GroupBy(entry => entry.Header)
                                .Where(group => group.Count() > 1)
                                .Select(group => group.Key);
        if (duplicates.Any())
            throw new XFEDictionaryException("Header的值应唯一");
        xFEDictionaryList.AddRange(collection);
    }
    /// <summary>
    /// 追加字典
    /// </summary>
    /// <param name="dictionaryString">字典字符串</param>
    public void AddRange(string dictionaryString)
    {
        xFEDictionaryList.AddRange(ToList(dictionaryString));
    }
    /// <summary>
    /// 插入条目
    /// </summary>
    /// <param name="index">插入位置</param>
    /// <param name="entry">条目</param>
    /// <exception cref="XFEDictionaryException"></exception>
    public void Insert(int index, XFEEntry entry)
    {
        if (Contains(entry.Header))
            throw new XFEDictionaryException("Header的值应唯一！");
        xFEDictionaryList.Insert(index, entry);
    }
    /// <summary>
    /// 插入条目
    /// </summary>
    /// <param name="index">插入位置</param>
    /// <param name="entryString">条目</param>
    /// <exception cref="XFEDictionaryException"></exception>
    public void Insert(int index, string entryString)
    {
        var entry = XFEEntry.ToEntry(entryString);
        if (entry is not null)
        {
            if (Contains(entry.Header))
                throw new XFEDictionaryException("Header的值应唯一！");
            xFEDictionaryList.Insert(index, entry);
        }
    }
    /// <summary>
    /// 插入条目
    /// </summary>
    /// <param name="index">插入位置</param>
    /// <param name="header">头</param>
    /// <param name="content">内容</param>
    /// <exception cref="XFEDictionaryException"></exception>
    public void Insert(int index, string header, string content)
    {
        if (Contains(header))
            throw new XFEDictionaryException("Header的值应唯一！");
        xFEDictionaryList.Insert(index, new XFEEntry(header, content));
    }
    /// <summary>
    /// 清空字典
    /// </summary>
    public void Clear()
    {
        xFEDictionaryList.Clear();
    }
    /// <summary>
    /// 检测是否包含
    /// </summary>
    /// <param name="item">条目对象</param>
    /// <returns></returns>
    public bool Contains(XFEEntry item)
    {
        return xFEDictionaryList.Contains(item);
    }
    /// <summary>
    /// 检测是否包含头
    /// </summary>
    /// <param name="headerString">指定头</param>
    /// <returns></returns>
    public bool Contains(string headerString)
    {
        return xFEDictionaryList.Find(x => x.Header == headerString) is not null;
    }
    /// <summary>
    /// 检测是否包含内容
    /// </summary>
    /// <param name="contentString">指定内容</param>
    /// <returns></returns>
    public bool ContainContent(string contentString)
    {
        return xFEDictionaryList.Find(x => x.Content == contentString) is not null;
    }
    /// <summary>
    /// 复制
    /// </summary>
    /// <param name="array">条目数组</param>
    /// <param name="arrayIndex">索引</param>
    public void CopyTo(XFEEntry[] array, int arrayIndex)
    {
        xFEDictionaryList.CopyTo(array, arrayIndex);
    }
    /// <summary>
    /// 获取索引器
    /// </summary>
    /// <returns></returns>
    public IEnumerator<XFEEntry> GetEnumerator()
    {
        return xFEDictionaryList.GetEnumerator();
    }
    /// <summary>
    /// 移除指定的条目
    /// </summary>
    /// <param name="item">条目对象</param>
    /// <returns></returns>
    public bool Remove(XFEEntry item)
    {
        return xFEDictionaryList.Remove(item);
    }
    /// <summary>
    /// 移除指定的条目
    /// </summary>
    /// <param name="headerString">头</param>
    /// <returns></returns>
    public bool Remove(string headerString)
    {
        var entry = xFEDictionaryList.Find(x => x.Header == headerString);
        if (entry is not null)
            return xFEDictionaryList.Remove(entry);
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
        xFEDictionaryList.RemoveRange(startIndex, count);
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
            return xFEDictionaryList.Find(x => x.Header == headerString)?.Content;
        }
        set
        {
            if (value is not null)
            {
                var entry = xFEDictionaryList.Find(x => x.Header == headerString);
                if (entry is not null)
                    entry.Content = value;
            }
        }
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return xFEDictionaryList.GetEnumerator();
    }
    /// <summary>
    /// 转换为字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string result = "";
        foreach (var item in xFEDictionaryList)
        {
            result += item.ToString();
        }
        return result;
    }
    /// <summary>
    /// 将字符串转换为XFEEntry的List
    /// </summary>
    /// <param name="dictionaryString">字符串</param>
    /// <returns></returns>
    /// <exception cref="XFEDictionaryException"></exception>
    protected static List<XFEEntry> ToList(string dictionaryString)
    {
        List<XFEEntry> xFEList = [];
        string[] messages = dictionaryString.Split(DictionarySeparator, StringSplitOptions.None);
        foreach (string message in messages)
        {
            string[] xFEDictionaryString = message.Split(EntrySeparator, StringSplitOptions.None);
            if (xFEDictionaryString.Length == 5)
            {
                xFEList.Add(new XFEEntry(xFEDictionaryString[1].Replace("[++", "[+").Replace("++]", "+]").Replace("|{++", "|{+").Replace("++}|", "+}|"), xFEDictionaryString[3].Replace("[++", "[+").Replace("++]", "+]").Replace("|{++", "|{+").Replace("++}|", "+}|")));
            }
        }
        var duplicates = xFEList.GroupBy(entry => entry.Header)
                                .Where(group => group.Count() > 1)
                                .Select(group => group.Key);
        if (duplicates.Any())
            throw new XFEDictionaryException("Header的值应唯一");
        return xFEList;
    }
    /// <summary>
    /// 将字符串转换为XFEDictionary
    /// </summary>
    /// <param name="dictionaryString">字符串</param>
    /// <returns></returns>
    /// <exception cref="XFEDictionaryException"></exception>
    public static XFEDictionary ToDictionary(string dictionaryString)
    {
        return new XFEDictionary(ToList(dictionaryString));
    }
    /// <summary>
    /// XFE字典存储
    /// </summary>
    /// <param name="xFEDictionaryList">条目列表</param>
    public XFEDictionary(List<XFEEntry> xFEDictionaryList)
    {
        this.xFEDictionaryList = xFEDictionaryList;
    }
    /// <summary>
    /// XFE字典存储
    /// </summary>
    /// <param name="headerAndContentStrings"></param>
    /// <exception cref="XFEDictionaryException"></exception>
    public XFEDictionary(params string[] headerAndContentStrings)
    {
        if (!(headerAndContentStrings.Length % 2 == 0))
            throw new XFEDictionaryException("输入的字符串数组格式不正确，应为Header，Content，Header，Content...");
        for (int i = 0; i < headerAndContentStrings.Length; i += 2)
        {
            xFEDictionaryList.Add(new XFEEntry(headerAndContentStrings[i], headerAndContentStrings[i + 1]));
        }
    }
    /// <summary>
    /// XFE字典存储
    /// </summary>
    /// <param name="dictionaryString">字典字符串</param>
    public XFEDictionary(string dictionaryString)
    {
        xFEDictionaryList = ToList(dictionaryString);
    }
    /// <summary>
    /// XFE字典存储
    /// </summary>
    public XFEDictionary() { }
    /// <summary>
    /// 从字符串加载字典
    /// </summary>
    /// <param name="dictionaryString"></param>
    public static implicit operator XFEDictionary(string dictionaryString) => new(dictionaryString);
    /// <summary>
    /// 将字典转为字符串
    /// </summary>
    /// <param name="xFEEntries"></param>
    public static implicit operator string(XFEDictionary xFEEntries) => new(xFEEntries.ToString());
}