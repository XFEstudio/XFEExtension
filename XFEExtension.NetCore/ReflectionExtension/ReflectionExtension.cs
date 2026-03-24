using System.Reflection;

namespace XFEExtension.NetCore.ReflectionExtension;

/// <summary>
/// 反射拓展
/// </summary>
public static class ReflectionExtension
{
    /// <summary>
    /// 获取方法默认参数
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static object?[] GetDefaultParameters(this MethodInfo method)
    {
        var parameters = method.GetParameters();
        object?[] defaultParameters = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            defaultParameters[i] = paramType.IsValueType ? Activator.CreateInstance(paramType) : null;
        }
        return defaultParameters;
    }

    /// <param name="obj"></param>
    extension(object obj)
    {
        /// <summary>
        /// 获取某个类中的某个私有字段的值
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="fieldName">字段名称</param>
        /// <returns></returns>
        public T? GetPrivateField<T>(string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T?)field?.GetValue(obj);
        }

        /// <summary>
        /// 设置某个类中的某个私有字段的值
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="value">值</param>
        public void SetPrivateField(string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        /// <summary>
        /// 获取某个类中的某个私有属性的值
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public T? GetPrivateProperty<T>(string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T?)property?.GetValue(obj);
        }

        /// <summary>
        /// 设置某个类中的某个私有属性的值
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="value">值</param>
        public void SetPrivateProperty(string propertyName, object value)
        {
            var property = obj.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            property?.SetValue(obj, value);
        }
    }

    /// <param name="type"></param>
    extension(Type type)
    {
        /// <summary>
        /// 获取某个类中的某个私有静态字段的值
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="fieldName">字段名称</param>
        /// <returns></returns>
        public T? GetPrivateStaticField<T>(string fieldName)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            return (T?)field?.GetValue(null);
        }

        /// <summary>
        /// 设置某个类中的某个私有静态字段的值
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="value">值</param>
        public void SetPrivateStaticField(string fieldName, object value)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            field?.SetValue(null, value);
        }

        /// <summary>
        /// 获取某个类中的某个私有静态属性的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public T? GetPrivateStaticProperty<T>(string propertyName)
        {
            var property = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
            return (T?)property?.GetValue(null);
        }

        /// <summary>
        /// 设置某个类中的某个私有静态属性的值
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="value">值</param>
        public void SetPrivateStaticProperty(string propertyName, object value)
        {
            var property = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
            property?.SetValue(null, value);
        }
    }

    /// <param name="obj"></param>
    extension(object obj)
    {
        /// <summary>
        /// 调用某个类中的某个私有方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodName">方法名称</param>
        /// <param name="parameters">传入的参数</param>
        /// <returns></returns>
        public T? InvokePrivateMethod<T>(string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T?)method?.Invoke(obj, parameters);
        }

        /// <summary>
        /// 调用某个类中的某个私有方法
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <param name="parameters">传入参数</param>
        public void InvokePrivateMethod(string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(obj, parameters);
        }
    }

    /// <summary>
    /// 调用某个类中的某个私有静态方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="methodName">方法名称</param>
    /// <param name="parameters">传入参数</param>
    /// <returns></returns>
    public static T? InvokePrivateStaticMethod<T>(this Type type, string methodName, params object[] parameters)
    {
        var method = type.GetMethod(methodName);
        return (T?)method?.Invoke(null, parameters);
    }
}
