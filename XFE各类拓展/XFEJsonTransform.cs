using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XFE各类拓展.XFEJsonTransform
{
    /// <summary>
    /// XFEJson转换器
    /// </summary>
    public static class XFEJsonTransformer
    {
        #region 对象转Json字符串
        /// <summary>
        /// 将对象转换为Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="XFEJsonTransformException">空对象异常</exception>
        public static string ConvertToJson(this object obj)
        {
            if (obj is null)
            {
                throw new XFEJsonTransformException("对象为空");
            }

            Type type = obj.GetType();

            if (IsSimpleType(type))
            {
                return GetSimpleValueAsString(obj);
            }

            if (type.IsArray || typeof(IEnumerable).IsAssignableFrom(type))
            {
                return ConvertArrayOrEnumerableToJson(obj);
            }

            return ConvertObjectToJson(obj);
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime);
        }

        private static string GetSimpleValueAsString(object value)
        {
            if (value is string)
            {
                return $"\"{value}\"";
            }

            if (value is DateTime dateTime)
            {
                return $"\"{dateTime:s}\"";
            }

            if (value is bool boolValue)
            {
                return boolValue ? "true" : "false";
            }

            return value.ToString();
        }


        private static string ConvertArrayOrEnumerableToJson(object obj)
        {
            var array = obj as IEnumerable;
            var jsonElements = new List<string>();

            foreach (var item in array)
            {
                jsonElements.Add(ConvertToJson(item));
            }

            return $"[{string.Join(",", jsonElements)}]";
        }

        private static string ConvertObjectToJson(object obj)
        {
            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead);

            var jsonProperties = new List<string>();

            foreach (var property in properties)
            {
                var propertyName = property.Name;
                var propertyValue = property.GetValue(obj);

                if (propertyValue is null)
                {
                    jsonProperties.Add($"\"{propertyName}\":null");
                }
                else
                {
                    if (property.PropertyType.IsEnum)
                    {
                        jsonProperties.Add($"\"{propertyName}\":{(int)propertyValue}");
                    }
                    else
                    {
                        jsonProperties.Add($"\"{propertyName}\":{ConvertToJson(propertyValue)}");
                    }
                }
            }

            return $"{{{string.Join(",", jsonProperties)}}}";
        }
        #endregion
        #region Json字符串转对象
        /// <summary>
        /// 将Json字符串转换为对象
        /// </summary>
        /// <typeparam name="T">待转换的类型</typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T ConvertFromJson<T>(string jsonString) where T : new()
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                throw new ArgumentException("Json字符串为空");
            }

            var jsonObject = (T)Activator.CreateInstance(typeof(T));

            var jsonProperties = GetJsonProperties(jsonString);
            var objectType = typeof(T);

            foreach (var jsonProperty in jsonProperties)
            {
                var propertyName = jsonProperty.Key;
                var propertyValue = jsonProperty.Value;

                var property = objectType.GetProperty(propertyName);
                if (property != null)
                {
                    var convertedValue = ConvertToPropertyValue(propertyValue, property.PropertyType);
                    property.SetValue(jsonObject, convertedValue);
                }
            }

            return jsonObject;
        }

        private static Dictionary<string, string> GetJsonProperties(string jsonString)
        {
            var jsonProperties = new Dictionary<string, string>();

            var jsonTokens = jsonString.Trim('{', '}').Split(',');

            foreach (var jsonToken in jsonTokens)
            {
                var propertyParts = jsonToken.Split(':');
                if (propertyParts.Length == 2)
                {
                    var propertyName = propertyParts[0].Trim('\"');
                    var propertyValue = propertyParts[1].Trim();

                    jsonProperties[propertyName] = propertyValue;
                }
            }

            return jsonProperties;
        }

        private static object ConvertToPropertyValue(string propertyValue, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return propertyValue.Trim('\"');
            }

            if (targetType == typeof(int))
            {
                return int.Parse(propertyValue);
            }

            if (targetType == typeof(decimal))
            {
                return decimal.Parse(propertyValue);
            }

            if (targetType == typeof(DateTime))
            {
                return DateTime.Parse(propertyValue.Trim('\"'));
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, propertyValue);
            }

            throw new NotSupportedException($"不支持的属性类型: {targetType.Name}");
        }

        #endregion
    }
}