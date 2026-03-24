using System.Reflection;

namespace XFEExtension.NetCore.AttributeExtension;

/// <summary>
/// 属性拓展类
/// </summary>
public static class AttributeExtension
{
    #region 类名方法
    /// <summary>
    /// 获取给定类的某个方法的给定属性的对象
    /// </summary>
    /// <typeparam name="T">属性类型</typeparam>
    /// <param name="className">类名(区分大小写)</param>
    /// <param name="methodName">类中的方法名</param>
    /// <returns></returns>
    public static T? GetAttributeInMethod<T>(string className, string methodName) where T : Attribute
    {
        var type = Assembly.GetCallingAssembly().GetType(className, false, true);
        if (type is null)
            throw new XFEExtensionException($"未找到类{className}");
        var method = type.GetMethod(methodName);
        if (method is null)
            throw new XFEExtensionException($"未找到方法{methodName}");
        return method.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
    }
    /// <summary>
    /// 获取给定类的某个方法的给定属性的对象列表
    /// </summary>
    /// <typeparam name="T">属性类型</typeparam>
    /// <param name="className">类名(区分大小写)</param>
    /// <param name="methodName">类中的方法名</param>
    /// <returns></returns>
    public static List<T?> GetAttributesInMethod<T>(string className, string methodName) where T : Attribute
    {
        var type = Assembly.GetCallingAssembly().GetType(className, false, true);
        if (type is null)
            throw new XFEExtensionException($"未找到类{className}");
        var method = type.GetMethod(methodName);
        return method is null ? throw new XFEExtensionException($"未找到方法{methodName}") : method.GetCustomAttributes(typeof(T), false).Select(x => x as T).ToList();
    }
    /// <summary>
    /// 获取给定类的某个字段的给定属性的对象
    /// </summary>
    /// <typeparam name="T">属性类型</typeparam>
    /// <param name="className">类名(区分大小写)</param>
    /// <param name="fieldName">类中的字段名称</param>
    /// <returns></returns>
    public static T? GetAttributeInField<T>(string className, string fieldName) where T : Attribute
    {
        var type = Assembly.GetCallingAssembly().GetType(className, false, true);
        if (type is null)
            throw new XFEExtensionException($"未找到类{className}");
        var field = type.GetField(fieldName);
        if (field is null)
            throw new XFEExtensionException($"未找到字段{fieldName}");
        return field.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
    }
    /// <summary>
    /// 获取给定类的某个字段的给定属性的对象列表
    /// </summary>
    /// <typeparam name="T">属性类型</typeparam>
    /// <param name="className">类名(区分大小写)</param>
    /// <param name="fieldName">类中的字段名称</param>
    /// <returns></returns>
    public static List<T?> GetAttributesInField<T>(string className, string fieldName) where T : Attribute
    {
        var type = Assembly.GetCallingAssembly().GetType(className, false, true);
        if (type is null)
            throw new XFEExtensionException($"未找到类{className}");
        var field = type.GetField(fieldName);
        return field is null ? throw new XFEExtensionException($"未找到字段{fieldName}") : field.GetCustomAttributes(typeof(T), false).Select(x => x as T).ToList();
    }
    /// <summary>
    /// 获取给定类的某个属性的给定属性的对象
    /// </summary>
    /// <typeparam name="T">属性类型</typeparam>
    /// <param name="className">类名(区分大小写)</param>
    /// <param name="propertyName">类中的属性名称</param>
    /// <returns></returns>
    public static T? GetAttributeInProperty<T>(string className, string propertyName) where T : Attribute
    {
        var type = Assembly.GetCallingAssembly().GetType(className, false, true);
        if (type is null)
            throw new XFEExtensionException($"未找到类{className}");
        var property = type.GetProperty(propertyName);
        if (property is null)
            throw new XFEExtensionException($"未找到属性{propertyName}");
        return property.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
    }
    /// <summary>
    /// 获取给定类的某个属性的给定属性的对象列表
    /// </summary>
    /// <typeparam name="T">属性类型</typeparam>
    /// <param name="className">类名(区分大小写)</param>
    /// <param name="propertyName">类中的属性名称</param>
    /// <returns></returns>
    public static List<T?> GetAttributesInProperty<T>(string className, string propertyName) where T : Attribute
    {
        var type = Assembly.GetCallingAssembly().GetType(className, false, true);
        if (type is null)
            throw new XFEExtensionException($"未找到类{className}");
        var property = type.GetProperty(propertyName);
        return property is null ? throw new XFEExtensionException($"未找到属性{propertyName}") : property.GetCustomAttributes(typeof(T), false).Select(x => x as T).ToList();
    }
    /// <summary>
    /// 获取给定类的给定方法的给定参数的给定属性的对象
    /// </summary>
    /// <typeparam name="T">属性类型</typeparam>
    /// <param name="className">类名(区分大小写)</param>
    /// <param name="methodName">方法名</param>
    /// <param name="paramName">参数名</param>
    /// <returns></returns>
    public static T? GetAttributeInParam<T>(string className, string methodName, string paramName) where T : Attribute
    {
        var type = Assembly.GetCallingAssembly().GetType(className, false, true);
        if (type is null)
            throw new XFEExtensionException($"未找到类{className}");
        var method = type.GetMethod(methodName);
        if (method is null)
            throw new XFEExtensionException($"未找到方法{methodName}");
        var param = method.GetParameters().FirstOrDefault(x => x.Name == paramName);
        if (param is null)
            throw new XFEExtensionException($"未找到参数{paramName}");
        return param.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
    }
    /// <summary>
    /// 获取给定类的给定方法的给定参数的给定属性的对象列表
    /// </summary>
    /// <typeparam name="T">属性类型</typeparam>
    /// <param name="className">类名(区分大小写)</param>
    /// <param name="methodName">方法名</param>
    /// <param name="paramName">参数名</param>
    /// <returns></returns>
    public static List<T?> GetAttributesInParam<T>(string className, string methodName, string paramName) where T : Attribute
    {
        var type = Assembly.GetCallingAssembly().GetType(className, false, true);
        if (type is null)
            throw new XFEExtensionException($"未找到类{className}");
        var method = type.GetMethod(methodName);
        if (method is null)
            throw new XFEExtensionException($"未找到方法{methodName}");
        var param = method.GetParameters().FirstOrDefault(x => x.Name == paramName);
        return param is null ? throw new XFEExtensionException($"未找到参数{paramName}") : param.GetCustomAttributes(typeof(T), false).Select(x => x as T).ToList();
    }
    /// <summary>
    /// 给定类的给定属性的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="className"></param>
    /// <returns></returns>
    public static T? GetAttributeOnClass<T>(string className) where T : Attribute
    {
        var type = Assembly.GetCallingAssembly().GetType(className, false, true);
        if (type is null)
            throw new XFEExtensionException($"未找到类{className}");
        return type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
    }
    /// <summary>
    /// 给定类的给定属性的对象列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="className"></param>
    /// <returns></returns>
    public static List<T?> GetAttributesOnClass<T>(string className) where T : Attribute
    {
        var type = Assembly.GetCallingAssembly().GetType(className, false, true);
        return type is null ? throw new XFEExtensionException($"未找到类{className}") : type.GetCustomAttributes(typeof(T), false).Select(x => x as T).ToList();
    }
    #endregion
    #region 拓展方法

    /// <param name="obj">对象</param>
    extension(object obj)
    {
        /// <summary>
        /// 获取该对象的给定属性的对象
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        public T? GetAttribute<T>() where T : Attribute
        {
            if (obj is string or int or double or float or decimal or DateTime or bool)
            {
                throw new XFEExtensionException("基础类型不支持获取属性");
            }
            return obj.GetType().GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        /// <summary>
        /// 获取该对象的给定属性的对象列表
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        public List<T?> GetAttributes<T>() where T : Attribute => obj is string or int or double or float or decimal or DateTime or bool ? throw new XFEExtensionException("基础类型不支持获取属性") : obj.GetType().GetCustomAttributes(typeof(T), false).Select(x => x as T).ToList();

        /// <summary>
        /// 获取给定类的某个方法的给定属性的对象
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="methodName">类中的方法名</param>
        /// <returns></returns>
        public T? GetAttributeInMethod<T>(string methodName) where T : Attribute
        {
            var className = obj.GetType().ToString();
            var type = Assembly.GetCallingAssembly().GetType(className, false, true) ?? throw new XFEExtensionException($"未找到类{className}");
            var method = type.GetMethod(methodName);
            if (method is null)
            {
                throw new XFEExtensionException($"未找到方法{methodName}");
            }
            return method.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        /// <summary>
        /// 获取给定类的某个方法的给定属性的对象列表
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="methodName">类中的方法名</param>
        /// <returns></returns>
        public List<T?> GetAttributesInMethod<T>(string methodName) where T : Attribute
        {
            var className = obj.GetType().ToString();
            var type = Assembly.GetCallingAssembly().GetType(className, false, true) ?? throw new XFEExtensionException($"未找到类{className}");
            var method = type.GetMethod(methodName);
            return method is null ? throw new XFEExtensionException($"未找到方法{methodName}") : method.GetCustomAttributes(typeof(T), false).Select(x => x as T).ToList();
        }

        /// <summary>
        /// 获取给定类的某个字段的给定属性的对象
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="fieldName">类中的字段名称</param>
        /// <returns></returns>
        public T? GetAttributeInField<T>(string fieldName) where T : Attribute
        {
            var className = obj.GetType().ToString();
            var type = Assembly.GetCallingAssembly().GetType(className, false, true) ?? throw new XFEExtensionException($"未找到类{className}");
            var field = type.GetField(fieldName);
            if (field is null)
                throw new XFEExtensionException($"未找到字段{fieldName}");
            return field.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        /// <summary>
        /// 获取给定类的某个字段的给定属性的对象列表
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="fieldName">类中的字段名称</param>
        /// <returns></returns>
        public List<T?> GetAttributesInField<T>(string fieldName) where T : Attribute
        {
            var className = obj.GetType().ToString();
            var type = Assembly.GetCallingAssembly().GetType(className, false, true) ?? throw new XFEExtensionException($"未找到类{className}");
            var field = type.GetField(fieldName);
            return field is null ? throw new XFEExtensionException($"未找到字段{fieldName}") : field.GetCustomAttributes(typeof(T), false).Select(x => x as T).ToList();
        }

        /// <summary>
        /// 获取给定类的某个属性的给定属性的对象
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="propertyName">类中的属性名称</param>
        /// <returns></returns>
        public T? GetAttributeInProperty<T>(string propertyName) where T : Attribute
        {
            var className = obj.GetType().ToString();
            var type = Assembly.GetCallingAssembly().GetType(className, false, true) ?? throw new XFEExtensionException($"未找到类{className}");
            var property = type.GetProperty(propertyName);
            return property is null ? throw new XFEExtensionException($"未找到属性{propertyName}") : property.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        /// <summary>
        /// 获取给定类的某个属性的给定属性的对象列表
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="propertyName">类中的属性名称</param>
        /// <returns></returns>
        public List<T?> GetAttributesInProperty<T>(string propertyName) where T : Attribute
        {
            var className = obj.GetType().ToString();
            var type = Assembly.GetCallingAssembly().GetType(className, false, true) ?? throw new XFEExtensionException($"未找到类{className}");
            var property = type.GetProperty(propertyName);
            return property is null ? throw new XFEExtensionException($"未找到属性{propertyName}") : property.GetCustomAttributes(typeof(T), false).Select(x => x as T).ToList();
        }

        /// <summary>
        /// 获取给定类的给定方法的给定参数的给定属性的对象
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="methodName">方法名</param>
        /// <param name="paramName">参数名</param>
        /// <returns></returns>
        public T? GetAttributeInParam<T>(string methodName, string paramName) where T : Attribute
        {
            var className = obj.GetType().ToString();
            var type = Assembly.GetCallingAssembly().GetType(className, false, true) ?? throw new XFEExtensionException($"未找到类{className}");
            var method = type.GetMethod(methodName);
            if (method is null)
                throw new XFEExtensionException($"未找到方法{methodName}");
            var param = method.GetParameters().FirstOrDefault(x => x.Name == paramName);
            if (param is null)
                throw new XFEExtensionException($"未找到参数{paramName}");
            return param.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        /// <summary>
        /// 获取给定类的给定方法的给定参数的给定属性的对象列表
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="methodName">方法名</param>
        /// <param name="paramName">参数名</param>
        /// <returns></returns>
        public List<T?> GetAttributesInParam<T>(string methodName, string paramName) where T : Attribute
        {
            var className = obj.GetType().ToString();
            var type = Assembly.GetCallingAssembly().GetType(className, false, true) ?? throw new XFEExtensionException($"未找到类{className}");
            var method = type.GetMethod(methodName);
            if (method is null)
            {
                throw new XFEExtensionException($"未找到方法{methodName}");
            }
            var param = method.GetParameters().FirstOrDefault(x => x.Name == paramName);
            return param is null ? throw new XFEExtensionException($"未找到参数{paramName}") : param.GetCustomAttributes(typeof(T), false).Select(x => x as T).ToList();
        }
    }
    #endregion
}
