using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

namespace XFEExtension.NetCore.XFETransform.StringConverter;

/// <summary>
/// 对象分析类
/// </summary>
public class ObjectAnalyzer : IStringConverter
{
    /// <summary>
    /// 分析对象
    /// </summary>
    /// <param name="objectInfo"></param>
    /// <returns></returns>
    public string OutPutObject(IObjectInfo objectInfo)
    {
        var outPutString = string.Empty;
        if (objectInfo.IsBasicType)
        {
            if (objectInfo.Value is null)
            {
                outPutString += $"{AddObjectPlace(objectInfo)} 空对象 {objectInfo.Name}：null\n";
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
                outPutString += $"""
                {AddObjectPlace(objectInfo)} {XFEConverter.OutPutTypeName(objectInfo.Type!)} {objectInfo.Name}
                ┌────┬────────────────────────────
                {(objectInfo.SubObjects is not null ? OutPutSubObjects(objectInfo.SubObjects) : "|    └─空对象")}
                └─────────────────────────────────

                """;
            }
            else
            {
                if (objectInfo.SubObjects is not null)
                    outPutString += OutPutSubObjects(objectInfo.SubObjects);
            }
        }
        return outPutString;
    }

    internal static string OutPutSubObjects(ISubObjects subObjects)
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
            if (obj.IsBasicType)
            {
                outString += currentConnectString;
                outString += obj.OutPutObject();
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
        ObjectPlace.List => "[列表]",
        ObjectPlace.Enum => "[枚举]",
        ObjectPlace.Other => "[其他]",
        _ => string.Empty,
    };
}
