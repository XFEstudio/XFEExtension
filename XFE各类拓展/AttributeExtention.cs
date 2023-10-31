using System;
using System.Linq;
using System.Reflection;

namespace XFE各类拓展.AttributeExtension
{
    /// <summary>
    /// 属性拓展类
    /// </summary>
    public static class AttributeExtension
    {
        /// <summary>
        /// 获取该对象的给定属性的对象
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="obj">对象</param>
        public static T GetAttribute<T>(this object obj) where T : Attribute
        {
            if (obj is string || obj is int || obj is double || obj is float || obj is decimal || obj is DateTime || obj is bool)
            {
                throw new XFEExtensionException("基础类型不支持获取属性");
            }
            return obj.GetType().GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }
        /// <summary>
        /// 获取给定类的某个方法的给定属性的对象
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="className">类名(区分大小写)</param>
        /// <param name="methodName">类中的方法名</param>
        /// <returns></returns>
        public static T GetAttributeInMethod<T>(string className, string methodName) where T : Attribute
        {
            var type = Assembly.GetCallingAssembly().GetType(className, false, true);
            if (type == null)
            {
                throw new XFEExtensionException($"未找到类{className}");
            }
            else
            {
                var method = type.GetMethod(methodName);
                if (method == null)
                {
                    throw new XFEExtensionException($"未找到方法{methodName}");
                }
                else
                {
                    return method.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
                }
            }
        }
        /// <summary>
        /// 获取给定类的某个字段的给定属性的对象
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="className">类名(区分大小写)</param>
        /// <param name="fieldName">类中的字段名称</param>
        /// <returns></returns>
        public static T GetAttributeInField<T>(string className, string fieldName) where T : Attribute
        {
            var type = Assembly.GetCallingAssembly().GetType(className, false, true);
            if (type == null)
            {
                throw new XFEExtensionException($"未找到类{className}");
            }
            else
            {
                var field = type.GetField(fieldName);
                if (field == null)
                {
                    throw new XFEExtensionException($"未找到字段{fieldName}");
                }
                else
                {
                    return field.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
                }
            }
        }
        /// <summary>
        /// 获取给定类的某个属性的给定属性的对象
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="className">类名(区分大小写)</param>
        /// <param name="propertyName">类中的属性名称</param>
        /// <returns></returns>
        public static T GetAttributeInProperty<T>(string className, string propertyName) where T : Attribute
        {
            var type = Assembly.GetCallingAssembly().GetType(className, false, true);
            if (type == null)
            {
                throw new XFEExtensionException($"未找到类{className}");
            }
            else
            {
                var property = type.GetProperty(propertyName);
                if (property == null)
                {
                    throw new XFEExtensionException($"未找到属性{propertyName}");
                }
                else
                {
                    return property.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
                }
            }
        }
        /// <summary>
        /// 给定类的给定属性的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="className"></param>
        /// <returns></returns>
        public static T GetAttributeOnClass<T>(string className) where T : Attribute
        {
            var type = Assembly.GetCallingAssembly().GetType(className, false, true);
            if (type == null)
            {
                throw new XFEExtensionException($"未找到类{className}");
            }
            else
            {
                return type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
            }
        }
    }
}
