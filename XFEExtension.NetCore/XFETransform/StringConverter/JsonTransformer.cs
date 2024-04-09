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
        var tabString = string.Empty;
        for (int k = 0; k < objectInfo.Layer; k++)
        {
            tabString += " ";
        }
        if (objectInfo.IsBasicType)
        {
            if (IsEnumerableMember(objectInfo))
            {
                if (objectInfo.Value is null)
                {
                    outPutString += $"{tabString}null";
                }
                else if (objectInfo.Value is string)
                {
                    outPutString += $"{tabString}\"{objectInfo.Value}\"";
                }
                else
                {
                    outPutString += $"{tabString}{objectInfo.Value}";
                }
            }
            else
            {
                if (objectInfo.Value is null)
                {
                    outPutString += $"{tabString}\"{objectInfo.Name}\": null";
                }
                else if (objectInfo.Value is string)
                {
                    outPutString += $"{tabString}\"{objectInfo.Name}\": \"{objectInfo.Value}\"";
                }
                else
                {
                    outPutString += $"{tabString}\"{objectInfo.Name}\": {objectInfo.Value}";
                }
            }
        }
        else
        {
            if (objectInfo.Layer == 0)
            {
                outPutString += objectInfo.SubObjects is not null ? $$"""
                    {{{OutPutSubObjects(objectInfo.SubObjects)}}
                    }
                    """ : "null";
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
            var tabString = string.Empty;
            for (int k = 0; k < obj.Layer; k++)
            {
                tabString += " ";
            }
            outString += "\n" + tabString;
            string? currentConnectString;
            if (i == subObjects.Count - 1)
            {
                currentConnectString = "";
            }
            else
            {
                currentConnectString = ",";
            }
            if (obj.IsBasicType)
            {
                outString += obj.OutPutObject();
                outString += currentConnectString;
            }
            else
            {
                if (IsEnumerableClassMember(obj))
                    outString += obj.Value is not null ? $$"""
                     {{tabString}}"{{obj.Name}}": [{{OutPutObject(obj)}}
                      {{tabString}}]{{currentConnectString}}
                     """ : "null";
                else if (IsEnumerableMember(obj))
                {
                    if (obj.Value is not null && (obj.Type!.IsAssignableTo(typeof(IEnumerable)) || obj.Type.IsAssignableTo(typeof(Array))))
                        outString += obj.Value is not null ? $$"""
                     {{tabString}}[{{OutPutObject(obj)}}
                       {{tabString}}]{{currentConnectString}}
                     """ : "null";
                    else
                        outString += obj.Value is not null ? $$"""
                     {{tabString}}{{{OutPutObject(obj)}}
                       {{tabString}}}{{currentConnectString}}
                     """ : "null";
                }
                else
                    outString += obj.Value is not null ? $$"""
                     {{tabString}}"{{obj.Name}}": {{{tabString}}{{OutPutObject(obj)}}
                      {{tabString}}}{{currentConnectString}}
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