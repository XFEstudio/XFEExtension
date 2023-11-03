using System;
using System.Collections.Generic;

namespace XFE各类拓展.ListExtension
{
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
                if (ary != null)
                    str += $"[+-{ary.ToString().Replace("[+", "[++").Replace("+]", "++]")}-+]";
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
                if (ary != null)
                    str += $"[+-{ary.GetType().GetProperty(propertyName).GetValue(ary).ToString().Replace("[+", "[++").Replace("+]", "++]")}-+]";
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
            string[] strings = str.Split(new string[] { "[+-", "-+]" }, StringSplitOptions.None);
            List<T> list = new List<T>();
            for (int i = 0; i < strings.Length; i++)
            {
                list.Add((T)Convert.ChangeType(strings[i].Replace("[++", "[+").Replace("++]", "+]"), typeof(T)));
            }
            return list;
        }
    }
}
