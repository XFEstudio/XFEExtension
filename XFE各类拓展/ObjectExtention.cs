using System;

namespace XFE各类拓展.ObjectExtension
{
    /// <summary>
    /// 所有类的基类的拓展
    /// </summary>
    public static class ObjectExtension
    {
        /// <summary>
        /// 进行浅拷贝
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns>浅拷贝后的对象</returns>
        /// <exception cref="ArgumentNullException">无类型错误</exception>
        public static T ActiveCopyOf<T>(this T source) where T : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return (T)source.GetType().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(source, null);
        }
        /// <summary>
        /// 进行静态拷贝
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns>静态拷贝后的对象</returns>
        public static T StaticCopyOf<T>(this T source) where T : class, new()
        {
            var properties = typeof(T).GetProperties();
            var newObject = new T();
            foreach (var property in properties)
            {
                property.SetValue(newObject, property.GetValue(source));
            }
            return newObject;
        }
    }
}