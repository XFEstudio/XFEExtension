using System.Collections;
using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

namespace XFEExtension.NetCore.XFETransform.StringConverter;

/// <summary>
/// Json转换器
/// </summary>
public class JsonTransformer : StringConverter
{
    /// <summary>
    /// 输出Json对象信息
    /// </summary>
    /// <param name="objectInfo"></param>
    /// <returns></returns>
    public override string OutPutObject(IObjectInfo objectInfo)
    {
        var outPutString = string.Empty;
        if (objectInfo.IsBasicType || objectInfo.ObjectPlace == ObjectPlace.Enum)
        {
            if (IsEnumerableMember(objectInfo))
            {
                if (objectInfo.Value is null)
                    outPutString += "null";
                else if (objectInfo.Value is string)
                    outPutString += $"\"{objectInfo.Value}\"";
                else if (objectInfo.ObjectPlace == ObjectPlace.Enum)
                    outPutString += $"{(int)objectInfo.Value}";
                else if (objectInfo.Value is bool)
                    outPutString += $"{objectInfo.Value.ToString()?.ToLower()}";
                else if (objectInfo.Type == typeof(int) || objectInfo.Type == typeof(double) || objectInfo.Type == typeof(float) || objectInfo.Type == typeof(decimal) || objectInfo.Type == typeof(long) || objectInfo.Type == typeof(short) || objectInfo.Type == typeof(byte) || objectInfo.Type == typeof(nint) || objectInfo.Type == typeof(uint) || objectInfo.Type == typeof(ulong) || objectInfo.Type == typeof(ushort) || objectInfo.Type == typeof(sbyte))
                    outPutString += objectInfo.Value;
                else
                    outPutString += $"\"{objectInfo.Value}\"";
            }
            else
            {
                if (objectInfo.Value is null)
                    outPutString += $"\"{objectInfo.Name}\":null";
                else if (objectInfo.Value is string)
                    outPutString += $"\"{objectInfo.Name}\":\"{objectInfo.Value}\"";
                else if (objectInfo.ObjectPlace == ObjectPlace.Enum)
                    outPutString += $"\"{objectInfo.Name}\":{(int)objectInfo.Value}";
                else if (objectInfo.Value is bool)
                    outPutString += $"\"{objectInfo.Name}\":{objectInfo.Value.ToString()?.ToLower()}";
                else if (objectInfo.Type == typeof(int) || objectInfo.Type == typeof(double) || objectInfo.Type == typeof(float) || objectInfo.Type == typeof(decimal) || objectInfo.Type == typeof(long) || objectInfo.Type == typeof(short) || objectInfo.Type == typeof(byte) || objectInfo.Type == typeof(nint) || objectInfo.Type == typeof(uint) || objectInfo.Type == typeof(ulong) || objectInfo.Type == typeof(ushort) || objectInfo.Type == typeof(sbyte))
                    outPutString += $"\"{objectInfo.Name}\":{objectInfo.Value}";
                else
                    outPutString += $"\"{objectInfo.Name}\":\"{objectInfo.Value}\"";
            }
        }
        else
        {
            if (objectInfo.Layer == 0)
            {
                if (objectInfo.IsArray)
                {
                    outPutString += objectInfo.SubObjects is not null ? $$"""
                    [{{OutPutSubObjects(objectInfo.SubObjects)}}]
                    """ : "null";
                }
                else
                {
                    outPutString += objectInfo.SubObjects is not null ? $$"""
                    {{{OutPutSubObjects(objectInfo.SubObjects)}}}
                    """ : "null";
                }
            }
            else
            {
                if (objectInfo.SubObjects is not null)
                    outPutString += OutPutSubObjects(objectInfo.SubObjects);
            }
        }
        return outPutString;
    }
    /// <summary>
    /// 输出子对象的Json信息
    /// </summary>
    /// <param name="subObjects"></param>
    /// <returns></returns>
    public override string OutPutSubObjects(ISubObjects subObjects)
    {
        var outString = string.Empty;
        for (int i = 0; i < subObjects.Count; i++)
        {
            var obj = subObjects[i];
            string? currentConnectString;
            if (i == subObjects.Count - 1)
            {
                currentConnectString = "";
            }
            else
            {
                currentConnectString = ",";
            }
            if (obj.IsBasicType || obj.ObjectPlace == ObjectPlace.Enum)
            {
                outString += obj.OutPutObject();
                outString += currentConnectString;
            }
            else
            {
                if (IsEnumerableClassMember(obj))
                    outString += obj.Value is not null ? $$"""
                     "{{obj.Name}}": [{{OutPutObject(obj)}}]{{currentConnectString}}
                     """ : "null";
                else if (IsEnumerableMember(obj))
                    if (obj.Value is not null && (obj.Type!.IsAssignableTo(typeof(IEnumerable)) || obj.Type.IsAssignableTo(typeof(Array))))
                        outString += obj.Value is not null ? $$"""
                     [{{OutPutObject(obj)}}]{{currentConnectString}}
                     """ : "null";
                    else
                        outString += obj.Value is not null ? $$"""
                     {{{OutPutObject(obj)}}}{{currentConnectString}}
                     """ : "null";
                else
                    outString += obj.Value is not null ? $$"""
                     "{{obj.Name}}": {{{OutPutObject(obj)}}}{{currentConnectString}}
                     """ : "null";
            }
        }
        if (outString.Length > 0 && outString[^1] == '\n')
        {
            outString = outString[..^1];
        }
        return outString;
    }

    internal static bool IsEnumerable(IObjectInfo objectInfo) => objectInfo.ObjectPlace == ObjectPlace.Array || objectInfo.ObjectPlace == ObjectPlace.List;
    internal static bool IsEnumerableMember(IObjectInfo objectInfo) => objectInfo.ObjectPlace == ObjectPlace.ArrayMember || objectInfo.ObjectPlace == ObjectPlace.ListMember;
    internal static bool IsEnumerableClassMember(IObjectInfo objectInfo) => objectInfo.Value is not null && objectInfo.ObjectPlace == ObjectPlace.Property && (objectInfo.Type!.IsAssignableTo(typeof(IEnumerable)) || objectInfo.Type.IsAssignableTo(typeof(Array)));
}