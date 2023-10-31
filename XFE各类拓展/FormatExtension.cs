using System;
using System.Collections.Generic;
using System.Linq;

namespace XFE各类拓展.FormatExtension
{
    /// <summary>
    /// XFE条目
    /// </summary>
    public class XFEEntry
    {
        /// <summary>
        /// 头
        /// </summary>
        public string Header { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
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
        public static XFEEntry ToEntry(string EntryString)
        {
            string[] messages = EntryString.Split(new string[] { "|{+-", "-+}|" }, StringSplitOptions.None);
            foreach (string message in messages)
            {
                string[] xFEDictionaryString = message.Split(new string[] { "[+-", "-+]" }, StringSplitOptions.None);
                if (xFEDictionaryString.Length == 5)
                {
                    return new XFEEntry(xFEDictionaryString[1].Replace("[++", "[+").Replace("++]", "+]").Replace("|{++", "|{+").Replace("++}|", "+}|"), xFEDictionaryString[3].Replace("[++", "[+").Replace("++]", "+]").Replace("|{++", "|{+").Replace("++}|", "+}|"));
                }
            }
            return null;
        }
        /// <summary>
        /// XFE条目
        /// </summary>
        /// <param name="Header">头</param>
        /// <param name="Content">内容</param>
        public XFEEntry(string Header, string Content)
        {
            this.Header = Header;
            this.Content = Content;
        }
    }
    /// <summary>
    /// XFE唯一字典存储
    /// </summary>
    public class XFEDictionary : ICollection<XFEEntry>
    {
        private List<XFEEntry> xFEDictionaryList = new List<XFEEntry>();
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
        /// <param name="DictionaryString"></param>
        public void Load(string DictionaryString)
        {
            xFEDictionaryList = ToList(DictionaryString);
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
        /// <param name="EntryString">条目字符串</param>
        /// <exception cref="XFEDictionaryException"></exception>
        public void Add(string EntryString)
        {
            var entry = XFEEntry.ToEntry(EntryString);
            if (Contains(entry.Header))
                throw new XFEDictionaryException("Header的值应唯一！");
            xFEDictionaryList.Add(entry);
        }
        /// <summary>
        /// 追加条目
        /// </summary>
        /// <param name="Header">头</param>
        /// <param name="Content">内容</param>
        /// <exception cref="XFEDictionaryException"></exception>
        public void Add(string Header, string Content)
        {
            if (Contains(Header))
                throw new XFEDictionaryException("Header的值应唯一！");
            xFEDictionaryList.Add(new XFEEntry(Header, Content));
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
        /// <param name="DictionaryString">字典字符串</param>
        public void AddRange(string DictionaryString)
        {
            xFEDictionaryList.AddRange(ToList(DictionaryString));
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
        /// <param name="EntryString">条目</param>
        /// <exception cref="XFEDictionaryException"></exception>
        public void Insert(int index, string EntryString)
        {
            var entry = XFEEntry.ToEntry(EntryString);
            if (Contains(entry.Header))
                throw new XFEDictionaryException("Header的值应唯一！");
            xFEDictionaryList.Insert(index, entry);
        }
        /// <summary>
        /// 插入条目
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="Header">头</param>
        /// <param name="Content">内容</param>
        /// <exception cref="XFEDictionaryException"></exception>
        public void Insert(int index, string Header, string Content)
        {
            if (Contains(Header))
                throw new XFEDictionaryException("Header的值应唯一！");
            xFEDictionaryList.Insert(index, new XFEEntry(Header, Content));
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
        /// <param name="HeaderString">指定头</param>
        /// <returns></returns>
        public bool Contains(string HeaderString)
        {
            return xFEDictionaryList.Find(x => x.Header == HeaderString) != null;
        }
        /// <summary>
        /// 检测是否包含内容
        /// </summary>
        /// <param name="ContentString">指定内容</param>
        /// <returns></returns>
        public bool ContainContent(string ContentString)
        {
            return xFEDictionaryList.Find(x => x.Content == ContentString) != null;
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
        /// <param name="HeaderString">头</param>
        /// <returns></returns>
        public bool Remove(string HeaderString)
        {
            return xFEDictionaryList.Remove(xFEDictionaryList.Find(x => x.Header == HeaderString));
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
        /// <param name="HeaderString">头</param>
        /// <returns></returns>
        public string this[string HeaderString]
        {
            get
            {
                return xFEDictionaryList.Find(x => x.Header == HeaderString)?.Content;
            }
            set
            {
                xFEDictionaryList.Find(x => x.Header == HeaderString).Content = value;
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
        /// <param name="DictionaryString">字符串</param>
        /// <returns></returns>
        /// <exception cref="XFEDictionaryException"></exception>
        protected static List<XFEEntry> ToList(string DictionaryString)
        {
            List<XFEEntry> xFEList = new List<XFEEntry>();
            string[] messages = DictionaryString.Split(new string[] { "|{+-", "-+}|" }, StringSplitOptions.None);
            foreach (string message in messages)
            {
                string[] xFEDictionaryString = message.Split(new string[] { "[+-", "-+]" }, StringSplitOptions.None);
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
        /// <param name="DictionaryString">字符串</param>
        /// <returns></returns>
        /// <exception cref="XFEDictionaryException"></exception>
        public static XFEDictionary ToDictionary(string DictionaryString)
        {
            return new XFEDictionary(ToList(DictionaryString));
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
        /// <param name="HeaderAndContentStrings"></param>
        /// <exception cref="XFEDictionaryException"></exception>
        public XFEDictionary(string[] HeaderAndContentStrings)
        {
            if (!(HeaderAndContentStrings.Length % 2 == 0))
                throw new XFEDictionaryException("输入的字符串数组格式不正确，应为Header，Content，Header，Content...");
            for (int i = 0; i < HeaderAndContentStrings.Length; i += 2)
            {
                xFEDictionaryList.Add(new XFEEntry(HeaderAndContentStrings[i], HeaderAndContentStrings[i + 1]));
            }
        }
        /// <summary>
        /// XFE字典存储
        /// </summary>
        /// <param name="DictionaryString">字典字符串</param>
        public XFEDictionary(string DictionaryString)
        {
            xFEDictionaryList = ToList(DictionaryString);
        }
        /// <summary>
        /// XFE字典存储
        /// </summary>
        public XFEDictionary() { }
    }
    /// <summary>
    /// XFE可重复字典存储
    /// </summary>
    public class XFEMultiDictionary : ICollection<XFEEntry>
    {
        private List<XFEEntry> xFEMultiDictionaryList = new List<XFEEntry>();
        /// <summary>
        /// 字典中的条目数
        /// </summary>
        public int Count => xFEMultiDictionaryList.Count;
        /// <summary>
        /// 只读
        /// </summary>
        public bool IsReadOnly => false;
        /// <summary>
        /// 从字符串中加载字典
        /// </summary>
        /// <param name="DictionaryString"></param>
        public void Load(string DictionaryString)
        {
            xFEMultiDictionaryList = ToList(DictionaryString);
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
        /// <param name="EntryString">条目字符串</param>
        public void Add(string EntryString)
        {
            xFEMultiDictionaryList.Add(XFEEntry.ToEntry(EntryString));
        }
        /// <summary>
        /// 追加条目
        /// </summary>
        /// <param name="Header">头</param>
        /// <param name="Content">内容</param>
        public void Add(string Header, string Content)
        {
            xFEMultiDictionaryList.Add(new XFEEntry(Header, Content));
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
        /// <param name="DictionaryString">字典字符串</param>
        public void AddRange(string DictionaryString)
        {
            xFEMultiDictionaryList.AddRange(ToList(DictionaryString));
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
        /// <param name="EntryString">条目</param>
        public void Insert(int index, string EntryString)
        {
            xFEMultiDictionaryList.Insert(index, XFEEntry.ToEntry(EntryString));
        }
        /// <summary>
        /// 插入条目
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="Header">头</param>
        /// <param name="Content">内容</param>
        public void Insert(int index, string Header, string Content)
        {
            xFEMultiDictionaryList.Insert(index, new XFEEntry(Header, Content));
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
        /// <param name="HeaderString">指定头</param>
        /// <returns></returns>
        public bool Contains(string HeaderString)
        {
            return xFEMultiDictionaryList.Find(x => x.Header == HeaderString) != null;
        }
        /// <summary>
        /// 检测是否包含内容
        /// </summary>
        /// <param name="ContentString">指定内容</param>
        /// <returns></returns>
        public bool ContainContent(string ContentString)
        {
            return xFEMultiDictionaryList.Find(x => x.Content == ContentString) != null;
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
        /// <param name="HeaderString">头</param>
        /// <returns></returns>
        public bool Remove(string HeaderString)
        {
            return xFEMultiDictionaryList.Remove(xFEMultiDictionaryList.Find(x => x.Header == HeaderString));
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
        /// <param name="HeaderString">头</param>
        /// <returns></returns>
        public string this[string HeaderString]
        {
            get
            {
                return xFEMultiDictionaryList.Find(x => x.Header == HeaderString)?.Content;
            }
            set
            {
                xFEMultiDictionaryList.Find(x => x.Header == HeaderString).Content = value;
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
        /// <param name="DictionaryString">字符串</param>
        /// <returns></returns>
        protected static List<XFEEntry> ToList(string DictionaryString)
        {
            List<XFEEntry> xFEList = new List<XFEEntry>();
            string[] messages = DictionaryString.Split(new string[] { "|{+-", "-+}|" }, StringSplitOptions.None);
            foreach (string message in messages)
            {
                string[] xFEDictionaryString = message.Split(new string[] { "[+-", "-+]" }, StringSplitOptions.None);
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
        /// <param name="DictionaryString">字符串</param>
        /// <returns></returns>
        public static XFEMultiDictionary ToMultiDictionary(string DictionaryString)
        {
            return new XFEMultiDictionary(ToList(DictionaryString));
        }
        /// <summary>
        /// XFE字典存储
        /// </summary>
        /// <param name="xFEDictionaryList">条目列表</param>
        public XFEMultiDictionary(List<XFEEntry> xFEDictionaryList)
        {
            this.xFEMultiDictionaryList = xFEDictionaryList;
        }
        /// <summary>
        /// XFE字典存储
        /// </summary>
        /// <param name="HeaderAndContentStrings"></param>
        /// <exception cref="XFEDictionaryException"></exception>
        public XFEMultiDictionary(string[] HeaderAndContentStrings)
        {
            if (!(HeaderAndContentStrings.Length % 2 == 0))
                throw new XFEDictionaryException("输入的字符串数组格式不正确，应为Header，Content，Header，Content...");
            for (int i = 0; i < HeaderAndContentStrings.Length; i += 2)
            {
                xFEMultiDictionaryList.Add(new XFEEntry(HeaderAndContentStrings[i], HeaderAndContentStrings[i + 1]));
            }
        }
        /// <summary>
        /// XFE字典存储
        /// </summary>
        /// <param name="DictionaryString">字典字符串</param>
        public XFEMultiDictionary(string DictionaryString)
        {
            xFEMultiDictionaryList = ToList(DictionaryString);
        }
        /// <summary>
        /// XFE字典存储
        /// </summary>
        public XFEMultiDictionary() { }
    }
}