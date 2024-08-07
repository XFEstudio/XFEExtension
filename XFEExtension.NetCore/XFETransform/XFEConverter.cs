﻿using System.Collections;
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
        return type == typeof(string) || type == typeof(int) || type == typeof(double) || type == typeof(float) || type == typeof(decimal) || type == typeof(long) || type == typeof(short) || type == typeof(byte) || type == typeof(bool) || type == typeof(char) || type == typeof(nint) || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte) || type == typeof(DateTime);
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
    /// 获取对象信息
    /// </summary>
    /// <param name="stringConverter">字符串转换器</param>
    /// <param name="name">对象名称</param>
    /// <param name="objectPlace">对象位置</param>
    /// <param name="layer">嵌套层数</param>
    /// <param name="type">类型</param>
    /// <param name="value">值</param>
    /// <param name="onlyProperty">是否只获取属性</param>
    /// <param name="onlyPublic">是否只获取公共成员</param>
    /// <returns></returns>
    public static IObjectInfo GetObjectInfo(IStringConverter? stringConverter, string? name, ObjectPlace objectPlace, int layer, Type? type, object? value = null, bool onlyProperty = true, bool onlyPublic = true)
    {
        if (value is null || type is null)
        {
            return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, true);
        }
        var isBasicType = IsBasicType(type);
        if (isBasicType)
            return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, true, value);
        else
        {
            if (type.IsAssignableTo(typeof(Array)))
            {
                var arrayObjects = new List<IObjectInfo>();
                foreach (var item in (Array)value)
                {
                    try
                    {
                        arrayObjects.Add(GetObjectInfo(stringConverter, "数组成员", ObjectPlace.ArrayMember, layer + 1, item.GetType(), item, onlyProperty, onlyPublic));
                    }
                    catch
                    {
                        arrayObjects.Add(GetObjectInfo(stringConverter, "数组成员", ObjectPlace.ArrayMember, layer + 1, null, null, onlyProperty, onlyPublic));
                    }
                }
                return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, false, true, value, arrayObjects);
            }
            else if (type.IsAssignableTo(typeof(IEnumerable)))
            {
                var enumerableObjects = new List<IObjectInfo>();
                foreach (var item in (IEnumerable)value)
                {
                    try
                    {
                        enumerableObjects.Add(GetObjectInfo(stringConverter, "列表成员", ObjectPlace.ListMember, layer + 1, item.GetType(), item, onlyProperty, onlyPublic));
                    }
                    catch
                    {
                        enumerableObjects.Add(GetObjectInfo(stringConverter, "列表成员", ObjectPlace.ListMember, layer + 1, null, null, onlyProperty, onlyPublic));
                    }
                }
                return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, false, true, value, enumerableObjects);
            }
            else if (type.IsAssignableTo(typeof(Enum)))
            {
                return new ObjectInfoImpl(stringConverter, name, ObjectPlace.Enum, layer, type, false, value);
            }
            else if (type.IsValueType)
            {
                return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, false, value);
            }
            var subObjects = new List<IObjectInfo>();
            foreach (var memberInfo in type.GetMembers((onlyPublic ? BindingFlags.Public : BindingFlags.NonPublic | BindingFlags.Public) | BindingFlags.Instance | BindingFlags.Static))
            {
                if (memberInfo is PropertyInfo propertyInfo)
                {
                    try
                    {
                        subObjects.Add(GetObjectInfo(stringConverter, propertyInfo.Name, ObjectPlace.Property, layer + 1, propertyInfo.PropertyType, propertyInfo.GetValue(value), onlyProperty, onlyPublic));
                    }
                    catch
                    {
                        subObjects.Add(GetObjectInfo(stringConverter, propertyInfo.Name, ObjectPlace.Property, layer + 1, propertyInfo.PropertyType, null, onlyProperty, onlyPublic));
                    }
                    continue;
                }
                if (!onlyProperty && memberInfo is FieldInfo fieldInfo)
                {
                    try
                    {
                        subObjects.Add(GetObjectInfo(stringConverter, fieldInfo.Name, ObjectPlace.Field, layer + 1, fieldInfo.FieldType, fieldInfo.GetValue(value), onlyProperty, onlyPublic));
                    }
                    catch
                    {
                        subObjects.Add(GetObjectInfo(stringConverter, fieldInfo.Name, ObjectPlace.Property, layer + 1, fieldInfo.FieldType, null, onlyProperty, onlyPublic));
                    }
                }
            }
            return new ObjectInfoImpl(stringConverter, name, objectPlace, layer, type, false, false, value, subObjects);
        }
    }
}