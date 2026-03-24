namespace XFEExtension.NetCore.ArrayExtension;

/// <summary>
/// 对数组的拓展
/// </summary>
public static class ArrayExtension
{
    private static readonly string[] Separator = ["[+-", "-+]"];

    /// <summary>
    /// 对数组的拓展
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arrays"></param>
    extension<T>(T[] arrays)
    {
        /// <summary>
        /// 将数组转换为XFE格式字符串
        /// </summary>
        /// <returns></returns>
        public string ToXFEString() => arrays.Aggregate(string.Empty, (current, ary) => current + $"[+-{ary?.ToString()?.Replace("[+", "[++").Replace("+]", "++]")}-+]");
    }

    /// <summary>
    /// 对数组的拓展
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arrays"></param>
    extension<T>(T[] arrays) where T : class
    {
        /// <summary>
        /// 将数组转换为XFE格式字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public string ToXFEString(string propertyName) => arrays.Aggregate(string.Empty, (current, ary) => current + $"[+-{ary?.GetType()?.GetProperty(propertyName)?.GetValue(ary)?.ToString()?.Replace("[+", "[++").Replace("+]", "++]")}-+]");
    }


    /// <param name="str"></param>
    extension(string str)
    {
        /// <summary>
        /// 将XFE格式字符串转换为数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T类型的数组</returns>
        public T[] ToXFEArray<T>()
        {
            string[] strings = str.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strings.Length; i++)
            {
                strings[i] = strings[i].Replace("[++", "[+").Replace("++]", "+]");
            }
            var array = new T[(strings.Length - 1) / 2];
            for (int i = 0, j = 1; i < array.Length; i++, j += 2)
            {
                array[i] = (T)Convert.ChangeType(strings[j], typeof(T));
            }
            return array;
        }
    }

    /// <param name="objects"></param>
    extension(object[]? objects)
    {
        /// <summary>
        /// 获取数组中的类型
        /// </summary>
        /// <returns></returns>
        public Type[]? GetTypes()
        {
            if (objects is null)
                return null;
            var types = new Type[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                types[i] = objects[i].GetType();
            }
            return types;
        }
    }
}