namespace XFE各类拓展.NetCore.ObjectExtension
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
            return source == null
                ? throw new ArgumentNullException(nameof(source))
                : (T)source.GetType().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(source, null)!;
        }
        /// <summary>
        /// 进行静态拷贝
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns>静态拷贝后的对象</returns>
        public static T? StaticCopyOf<T>(this T source) where T : class
        {
            if (source == null)
            {
                return default;
            }
            var fields = typeof(T).GetFields();
            var properties = typeof(T).GetProperties();
            var newObject = Activator.CreateInstance<T>();
            foreach (var property in properties)
            {
                property.SetValue(newObject, property.GetValue(source));
            }
            foreach (var field in fields)
            {
                field.SetValue(newObject, field.GetValue(source));
            }
            return newObject;
        }
        /// <summary>
        /// 比较两个对象的属性是否完全相同而非对象本身相同
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool AboutEqual<T>(this T obj1, T obj2)
        {
            if (obj1 == null && obj2 == null)
            {
                return true;
            }

            if (obj1 == null || obj2 == null)
            {
                return false;
            }

            Type type = typeof(T);
            var properties = type.GetProperties();
            var fields = type.GetFields();
            foreach (var property in properties)
            {
                var value1 = property.GetValue(obj1);
                var value2 = property.GetValue(obj2);

                if (!Equals(value1, value2))
                {
                    return false;
                }
            }
            foreach (var field in fields)
            {
                var value1 = field.GetValue(obj1);
                var value2 = field.GetValue(obj2);

                if (!Equals(value1, value2))
                {
                    return false;
                }
            }
            return true;
        }
    }
}