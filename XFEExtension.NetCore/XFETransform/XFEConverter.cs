using System.Collections;
using System.Reflection;
using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;
using XFEExtension.NetCore.XFETransform.StringConverter;

namespace XFEExtension.NetCore.XFETransform;

/// <summary>
/// XFE转换器
/// </summary>
public class XFEConverter
{
    /// <summary>
    /// 判断是否为基础类型
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static bool IsBasicType(Type type)
    {
        return type == typeof(string) || type == typeof(int) || type == typeof(double) || type == typeof(float) || type == typeof(decimal) || type == typeof(long) || type == typeof(short) || type == typeof(byte) || type == typeof(bool) || type == typeof(char) || type == typeof(nint) || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte) || type == typeof(DateTime) || type == typeof(TimeSpan);
    }

    /// <summary>
    /// 将基础类的文本转换为代码类型
    /// </summary>
    /// <param name="originString">原文本</param>
    /// <returns></returns>
    public static string ConvertBasicTypeToCodeType(string originString) => originString.ToLower().Replace("int32", "int").Replace("intptr", "nint").Replace("int64", "long").Replace("int16", "short").Replace("boolean", "bool").Replace("datetime", "DateTime");

    /// <summary>
    /// 输出类型名称
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static string OutPutTypeName(Type type)
    {
        if (IsBasicType(type))
            return ConvertBasicTypeToCodeType(type.Name);
        var genericArguments = type.GetGenericArguments();
        if (genericArguments.Length == 0)
            return type.Name;
        return $"{type.Name.Replace($"`{genericArguments.Length}", string.Empty)}<{string.Join(", ", genericArguments.Select(x =>
        {
            if (IsBasicType(x))
                return ConvertBasicTypeToCodeType(x.Name);
            return OutPutTypeName(x);
        }))}>";
    }

    /// <summary>
    /// 添加并赋值父类列表
    /// </summary>
    /// <param name="originalFatherList"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static List<object> AddAndCopyFatherList(List<object> originalFatherList, object value)
    {
        var newList = new List<object>();
        newList.AddRange(originalFatherList);
        newList.Add(value);
        return newList;
    }

    /// <summary>
    /// 获取对象信息
    /// </summary>
    /// <param name="stringConverter">字符串转换器</param>
    /// <param name="name">对象名称</param>
    /// <param name="objectPlace">对象位置</param>
    /// <param name="layer">嵌套层数</param>
    /// <param name="fatherObjectList">父类列表</param>
    /// <param name="type">类型</param>
    /// <param name="value">值</param>
    /// <param name="onlyProperty">是否只获取属性</param>
    /// <param name="onlyPublic">是否只获取公共成员</param>
    /// <param name="stopInThisLayer">是否在该层停止递归</param>
    /// <param name="maxLayer">最大解析深度</param>
    /// <param name="maxArrayCount">数组列表的最大解析数量</param>
    /// <returns></returns>
    public static IObjectInfo GetObjectInfo(IStringConverter? stringConverter, string? name, ObjectPlace objectPlace, int layer, List<object> fatherObjectList, Type? type, object? value = null, bool onlyProperty = true, bool onlyPublic = true, bool stopInThisLayer = false, int maxLayer = 10, int maxArrayCount = 999)
    {
        try
        {
            if (value is null || type is null)
            {
                return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, true);
            }
            if (fatherObjectList.Contains(value) && layer > 0)
            {
                return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, true, "重复递归异常");
            }
            if (layer > maxLayer)
            {
                return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, true, $"超过最大解析深度（{maxLayer}）");
            }
            var currentFatherList = AddAndCopyFatherList(fatherObjectList, value);
            var isBasicType = IsBasicType(type);
            if (isBasicType)
                return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, true, value);
            else
            {
                if (type.IsAssignableTo(typeof(Enum)))
                {
                    return new ObjectInfoImpl(stringConverter, name, ObjectPlace.Enum, layer, type, false, value);
                }
                else if (value is Exception exception)
                {
                    return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, false, exception.ToString());
                }
                else if (type.IsAssignableTo(typeof(Array)))
                {
                    var arrayObjects = new List<IObjectInfo>();
                    var currentCount = 0;
                    foreach (var item in (Array)value)
                    {
                        currentCount++;
                        try
                        {
                            arrayObjects.Add(GetObjectInfo(stringConverter, "数组成员", ObjectPlace.ArrayMember, layer + 1, currentFatherList, item.GetType(), item, onlyProperty, onlyPublic, stopInThisLayer, maxLayer, maxArrayCount));
                        }
                        catch
                        {
                            arrayObjects.Add(GetObjectInfo(stringConverter, "数组成员", ObjectPlace.ArrayMember, layer + 1, currentFatherList, null, null, onlyProperty, onlyPublic, stopInThisLayer, maxLayer, maxArrayCount));
                        }
                        if (currentCount > maxArrayCount)
                        {
                            arrayObjects.Add(new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, true, $"数组数量超出设定最大值（{maxArrayCount}）"));
                            break;
                        }
                    }
                    return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, false, true, value, arrayObjects);
                }
                else if (type.IsAssignableTo(typeof(IEnumerable)))
                {
                    var enumerableObjects = new List<IObjectInfo>();
                    var currentCount = 0;
                    foreach (var item in (IEnumerable)value)
                    {
                        currentCount++;
                        try
                        {
                            enumerableObjects.Add(GetObjectInfo(stringConverter, "列表成员", ObjectPlace.ListMember, layer + 1, currentFatherList, item.GetType(), item, onlyProperty, onlyPublic, stopInThisLayer, maxLayer, maxArrayCount));
                        }
                        catch
                        {
                            enumerableObjects.Add(GetObjectInfo(stringConverter, "列表成员", ObjectPlace.ListMember, layer + 1, currentFatherList, null, null, onlyProperty, onlyPublic, stopInThisLayer, maxLayer, maxArrayCount));
                        }
                        if (currentCount > maxArrayCount)
                        {
                            enumerableObjects.Add(new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, true, $"列表数量超出设定最大值（{maxArrayCount}）"));
                            break;
                        }
                    }
                    return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, false, true, value, enumerableObjects);
                }
                if (stopInThisLayer)
                {
                    return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, false, false, value);
                }
                var isType = value is Type || type.FullName == "System.RuntimeType";
                var subObjects = new List<IObjectInfo>();
                foreach (var memberInfo in type.GetMembers((onlyPublic ? BindingFlags.Public : BindingFlags.NonPublic | BindingFlags.Public) | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (memberInfo is PropertyInfo propertyInfo)
                    {
                        try
                        {
                            subObjects.Add(GetObjectInfo(stringConverter, propertyInfo.Name, ObjectPlace.Property, layer + 1, currentFatherList, propertyInfo.PropertyType, propertyInfo.GetValue(value), onlyProperty, onlyPublic, isType, maxLayer, maxArrayCount));
                        }
                        catch
                        {
                            subObjects.Add(GetObjectInfo(stringConverter, propertyInfo.Name, ObjectPlace.Property, layer + 1, currentFatherList, propertyInfo.PropertyType, null, onlyProperty, onlyPublic, isType, maxLayer, maxArrayCount));
                        }
                        continue;
                    }
                    if (!onlyProperty && memberInfo is FieldInfo fieldInfo)
                    {
                        try
                        {
                            subObjects.Add(GetObjectInfo(stringConverter, fieldInfo.Name, ObjectPlace.Field, layer + 1, currentFatherList, fieldInfo.FieldType, fieldInfo.GetValue(value), onlyProperty, onlyPublic, isType, maxLayer, maxArrayCount));
                        }
                        catch
                        {
                            subObjects.Add(GetObjectInfo(stringConverter, fieldInfo.Name, ObjectPlace.Property, layer + 1, currentFatherList, fieldInfo.FieldType, null, onlyProperty, onlyPublic, isType, maxLayer, maxArrayCount));
                        }
                    }
                }
                return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, false, false, value, subObjects);
            }
        }
        catch (Exception ex)
        {
            return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, true, $"无法获取：{ex.Message} ");
        }
    }
}