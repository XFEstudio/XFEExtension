using System;
using System.Collections.Generic;

namespace XFE各类拓展.ArrayExtension
{
    namespace AI
    {
        /// <summary>
        /// 对神经网络算法的数组矩阵的拓展
        /// </summary>
        public static class MatrixOfArrayExtension
        {
            /// <summary>
            /// 以矩阵形式打印数组
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="arrays"></param>
            public static void OutPutInMatrix<T>(this T[] arrays)
            {
                int i;
                Console.Write("[");
                for (i = 0; i < arrays.GetLength(0) - 1; i++)
                {
                    Console.Write($"{arrays[i]}\t");
                }
                Console.Write($"{arrays[i]}]\n");
            }
            /// <summary>
            /// 以矩阵形式打印数组
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="arrays"></param>
            public static void OutPutInMatrix<T>(this T[][] arrays)
            {
                for (int i = 0; i < arrays.GetLength(0); i++)
                {
                    int j;
                    Console.Write("[");
                    for (j = 0; j < arrays.GetLength(1) - 1; j++)
                    {
                        Console.Write($"{arrays[i][j]}\t");
                    }
                    Console.Write($"{arrays[i][j++]}]\n");
                }
            }
            /// <summary>
            /// 以矩阵形式打印数组
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="arrays"></param>
            public static void OutPutInMatrix<T>(this T[,] arrays)
            {
                for (int i = 0; i < arrays.GetLength(0); i++)
                {
                    int j;
                    Console.Write("[");
                    for (j = 0; j < arrays.GetLength(1) - 1; j++)
                    {
                        Console.Write($"{arrays[i, j]}\t");
                    }
                    Console.Write($"{arrays[i, j++]}]\n");
                }
            }
        }
    }
    /// <summary>
    /// 对数组的拓展
    /// </summary>
    public static class ArrayExtension
    {
        /// <summary>
        /// 将数组转换为XFE格式字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrays"></param>
        /// <returns></returns>
        public static string ToXFEString<T>(this T[] arrays)
        {
            string str = string.Empty;
            foreach (var ary in arrays)
            {
                if (ary != null)
                    str += $"[+-{ary.ToString().Replace("[+", "[++").Replace("+]", "++]")}-+]";
            }
            return str;
        }
        /// <summary>
        /// 将数组转换为XFE格式字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrays"></param>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public static string ToXFEString<T>(this T[] arrays, string propertyName) where T : class
        {
            string str = string.Empty;
            foreach (var ary in arrays)
            {
                if (ary != null)
                    str += $"[+-{ary.GetType().GetProperty(propertyName).GetValue(ary).ToString().Replace("[+", "[++").Replace("+]", "++]")}-+]";
            }
            return str;
        }
        /// <summary>
        /// 将XFE格式字符串转换为数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns>T类型的数组</returns>
        public static T[] ToXFEArray<T>(this string str)
        {
            string[] strings = str.Split(new string[] { "[+-", "-+]" }, StringSplitOptions.None);
            for (int i = 0; i < strings.Length; i++)
            {
                strings[i] = strings[i].Replace("[++", "[+").Replace("++]", "+]");
            }
            T[] array = new T[(strings.Length - 1) / 2];
            for (int i = 0, j = 1; i < array.Length; i++, j += 2)
            {
                array[i] = (T)Convert.ChangeType(strings[j], typeof(T));
            }
            return array;
        }
        /// <summary>
        /// 获取数组中的类型
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static Type[] GetTypes(this object[] objects)
        {
            if (objects is null)
                return null;
            Type[] types = new Type[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                types[i] = objects[i].GetType();
            }
            return types;
        }
    }
}