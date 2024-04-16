using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

namespace XFEExtension.NetCore.XFETransform.StringConverter;

/// <summary>
/// 对象分析类
/// </summary>
public class ObjectAnalyzer : StringConverter
{
    /// <summary>
    /// 分析对象
    /// </summary>
    /// <param name="objectInfo"></param>
    /// <returns></returns>
    public override string OutPutObject(IObjectInfo objectInfo)
    {
        var outPutString = string.Empty;
        if (objectInfo.IsBasicType || objectInfo.ObjectPlace == ObjectPlace.Enum)
        {
            if (objectInfo.Value is null)
            {
                outPutString += $"{AddObjectPlace(objectInfo)} {XFEConverter.OutPutTypeName(objectInfo.Type!)} {objectInfo.Name}：null\n";
            }
            else if (objectInfo.ObjectPlace == ObjectPlace.Enum)
            {
                outPutString += $"{AddObjectPlace(objectInfo)} {XFEConverter.OutPutTypeName(objectInfo.Type!)} {objectInfo.Name}：{objectInfo.Value}[{(int)objectInfo.Value}]\n";
            }
            else if (objectInfo.Value is string str && str == string.Empty)
            {
                outPutString += $"{AddObjectPlace(objectInfo)} {XFEConverter.OutPutTypeName(objectInfo.Type!)} {objectInfo.Name}：string.Empty\n";
            }
            else
            {
                outPutString += $"{AddObjectPlace(objectInfo)} {XFEConverter.OutPutTypeName(objectInfo.Type!)} {objectInfo.Name}：{objectInfo.Value}\n";
            }
        }
        else
        {
            if (objectInfo.Layer == 0)
            {
                if (objectInfo.SubObjects?.Count > 0)
                {
                    outPutString += $"""
                        {AddObjectPlace(objectInfo)} {XFEConverter.OutPutTypeName(objectInfo.Type!)} {objectInfo.Name}
                        ┌────┬────────────────────────────
                        {OutPutSubObjects(objectInfo.SubObjects)}
                        └─────────────────────────────────
                        """;
                }
                else
                {
                    outPutString += $"{AddObjectPlace(objectInfo)} {XFEConverter.OutPutTypeName(objectInfo.Type!)} {objectInfo.Name}: null";
                }
            }
            else
            {
                var tabString = string.Empty;
                for (int k = 0; k < objectInfo.Layer; k++)
                {
                    tabString += "│    ";
                }
                if (objectInfo.SubObjects is not null)
                {
                    if (objectInfo.SubObjects.Count > 0)
                        outPutString += OutPutSubObjects(objectInfo.SubObjects);
                    else
                        outPutString += $"{tabString}│    └─无内容";

                }
            }
        }
        return outPutString;
    }

    /// <summary>
    /// 分析子对象信息
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
                tabString += "│    ";
            }
            outString += tabString;
            string? currentConnectString;
            if (i == subObjects.Count - 1)
            {
                currentConnectString = "└─";
            }
            else
            {
                currentConnectString = "├─";
            }
            if (obj.IsBasicType || obj.ObjectPlace == ObjectPlace.Enum)
            {
                outString += currentConnectString;
                outString += obj.OutPutObject();
            }
            else
            {
                if (obj.SubObjects is null)
                {
                    outString += $"{currentConnectString}{AddObjectPlace(obj)} {XFEConverter.OutPutTypeName(obj.Type!)} {obj.Name}: null\n";
                }
                else
                {
                    outString += $"""
                     ├─{AddObjectPlace(obj)} {XFEConverter.OutPutTypeName(obj.Type!)} {obj.Name}
                     {tabString}├────┬────────────────────────────
                     {obj.OutPutObject()}
                     {tabString}{currentConnectString}────────────────────────────────

                     """;
                }
            }
        }
        if (outString.Length > 0 && outString[^1] == '\n')
        {
            outString = outString[..^1];
        }
        return outString;
    }

    internal static string AddObjectPlace(IObjectInfo objectInfo) => objectInfo.ObjectPlace switch
    {
        ObjectPlace.Property => "[属性]",
        ObjectPlace.Field => "[字段]",
        ObjectPlace.Main => "[主体]",
        ObjectPlace.Array => "[数组]",
        ObjectPlace.ArrayMember => "[数组成员]",
        ObjectPlace.List => "[列表]",
        ObjectPlace.ListMember => "[列表成员]",
        ObjectPlace.Enum => "[枚举]",
        ObjectPlace.Other => "[其他]",
        _ => string.Empty,
    };
}
