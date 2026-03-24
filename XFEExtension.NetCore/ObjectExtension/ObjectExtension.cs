using System.Reflection;

namespace XFEExtension.NetCore.ObjectExtension;

/// <summary>
/// 所有类的基类的拓展
/// </summary>
public static class ObjectExtension
{
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    extension<T>(T? source) where T : class
    {
        /// <summary>
        /// 进行浅拷贝
        /// </summary>
        /// <returns>浅拷贝后的对象</returns>
        /// <exception cref="ArgumentNullException">无类型错误</exception>
        public T ActiveCopyOf() => source is null ? throw new ArgumentNullException(nameof(source)) : (T)source.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(source, null)!;

        /// <summary>
        /// 进行静态拷贝
        /// </summary>
        /// <returns>静态拷贝后的对象</returns>
        public T? StaticCopyOf()
        {
            if (source is null)
                return null;
            var fields = typeof(T).GetFields();
            var properties = typeof(T).GetProperties();
            var newObject = Activator.CreateInstance<T>();
            foreach (var property in properties)
                property.SetValue(newObject, property.GetValue(source));
            foreach (var field in fields)
                field.SetValue(newObject, field.GetValue(source));
            return newObject;
        }

        /// <summary>
        /// 比较两个对象的属性是否完全相同而非对象本身相同
        /// </summary>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public bool AboutEqual(T? obj2)
        {
            if (source is null && obj2 is null)
                return true;
            if (source is null || obj2 is null)
                return false;
            var type = typeof(T);
            var properties = type.GetProperties();
            var fields = type.GetFields();
            if ((from property in properties let value1 = property.GetValue(source) let value2 = property.GetValue(obj2) where !Equals(value1, value2) select value1).Any())
                return false;
            return !(from field in fields let value1 = field.GetValue(source) let value2 = field.GetValue(obj2) where !Equals(value1, value2) select value1).Any();
        }
    }
}