using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

namespace XFEExtension.NetCore.XFETransform.StringConverter;

/// <summary>
/// 对象分析类
/// </summary>
public class ColoredObjectAnalyzer : StringConverter
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
                outPutString += $"{AddObjectPlace(objectInfo)} [color #4ec9b0]{XFEConverter.OutPutTypeName(objectInfo.Type!)} {AddObjectPlaceColor(objectInfo)}{objectInfo.Name}[color white]：[color #569cd6]null[color white]\n";
            }
            else if (objectInfo.ObjectPlace == ObjectPlace.Enum)
            {
                outPutString += $"{AddObjectPlace(objectInfo)} [color #b5cea8]{XFEConverter.OutPutTypeName(objectInfo.Type!)} [color white]{objectInfo.Name}：[color #d69d85]{objectInfo.Value}[color #569cd6][[color #b5cea8]{(int)objectInfo.Value}[color #569cd6]][color white]\n";
            }
            else if (objectInfo.Value is string str && str == string.Empty)
            {
                outPutString += $"{AddObjectPlace(objectInfo)} [color #569cd6]{XFEConverter.OutPutTypeName(objectInfo.Type!)}[color white] {AddObjectPlaceColor(objectInfo)}{objectInfo.Name}[color white]：[color #569cd6]string[color white].Empty\n";
            }
            else if(objectInfo.Value is string)
            {
                outPutString += $"{AddObjectPlace(objectInfo)} [color #569cd6]{XFEConverter.OutPutTypeName(objectInfo.Type!)}[color white] {AddObjectPlaceColor(objectInfo)}{objectInfo.Name}[color white]：[color #d69d85]{objectInfo.Value}[color white]\n";
            }
            else if(objectInfo.Value is float || objectInfo.Value is int || objectInfo.Value is double || objectInfo.Value is long || objectInfo.Value is short || objectInfo.Value is byte || objectInfo.Value is uint || objectInfo.Value is ulong || objectInfo.Value is ushort || objectInfo.Value is decimal)
            {
                outPutString += $"{AddObjectPlace(objectInfo)} [color #569cd6]{XFEConverter.OutPutTypeName(objectInfo.Type!)}[color white] {AddObjectPlaceColor(objectInfo)}{objectInfo.Name}[color white]：[color #b5cea8]{objectInfo.Value}[color white]\n";
            }
            else
            {
                outPutString += $"{AddObjectPlace(objectInfo)} [color #569cd6]{XFEConverter.OutPutTypeName(objectInfo.Type!)}[color white] {AddObjectPlaceColor(objectInfo)}{objectInfo.Name}[color white]：[color #569cd6]{objectInfo.Value}[color white]\n";
            }
        }
        else
        {
            if (objectInfo.Layer == 0)
            {
                if(objectInfo.SubObjects == null)
                {
                    outPutString += $"{AddObjectPlace(objectInfo)} [color #4ec9b0]{XFEConverter.OutPutTypeName(objectInfo.Type!)} {AddObjectPlaceColor(objectInfo)}{objectInfo.Name}: [color #569cd6]null[color white]";
                }
                else if (objectInfo.SubObjects?.Count > 0)
                {
                    outPutString += $"""
                        {AddObjectPlace(objectInfo)} [color #4ec9b0]{XFEConverter.OutPutTypeName(objectInfo.Type!)} {AddObjectPlaceColor(objectInfo)}{objectInfo.Name}
                        ┌────┬────────────────────────────
                        {OutPutSubObjects(objectInfo.SubObjects)}
                        └─────────────────────────────────
                        """;
                }
                else
                {
                    outPutString += $"""
                        {AddObjectPlace(objectInfo)} [color #4ec9b0]{XFEConverter.OutPutTypeName(objectInfo.Type!)} {AddObjectPlaceColor(objectInfo)}{objectInfo.Name}
                        ┌────┬────────────────────────────
                        │    └─[color #569cd6][无内容][color white]"
                        └─────────────────────────────────
                        """;
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
                        outPutString += $"{tabString}│    └─[color #569cd6][无内容][color white]";

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
                    outString += $"{currentConnectString}{AddObjectPlace(obj)} [color #4ec9b0]{XFEConverter.OutPutTypeName(obj.Type!)} {AddObjectPlaceColor(obj)}{obj.Name}: [color #569cd6]null[color white]\n";
                }
                else
                {
                    outString += $"""
                     ├─{AddObjectPlace(obj)} [color #4ec9b0]{XFEConverter.OutPutTypeName(obj.Type!)} {AddObjectPlaceColor(obj)}{obj.Name}
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
        ObjectPlace.Property => "[color #5695b6][[color white]属性[color #5695b6]]",
        ObjectPlace.Field => "[color #5695b6][[color white]字段[color #5695b6]]",
        ObjectPlace.Main => "[color #5695b6][[color white]主体[color #5695b6]]",
        ObjectPlace.Array => "[color #5695b6][[color white]数组[color #5695b6]]",
        ObjectPlace.ArrayMember => "[color #5695b6][[color white]成员[color #5695b6]]",
        ObjectPlace.List => "[color #5695b6][[color white]列表[color #5695b6]]",
        ObjectPlace.ListMember => "[color #5695b6][[color white]成员[color #5695b6]]",
        ObjectPlace.Enum => "[color #5695b6][[color white]枚举[color #5695b6]]",
        ObjectPlace.Other => "[color #5695b6][[color white]其他]",
        _ => string.Empty
    }; internal static string AddObjectPlaceColor(IObjectInfo objectInfo) => objectInfo.ObjectPlace switch
    {
        ObjectPlace.Property => "[color white]",
        ObjectPlace.Field => "[color #9898e7]",
        ObjectPlace.Main => "[color white]",
        ObjectPlace.Array => "[color white]",
        ObjectPlace.ArrayMember => "[color white]",
        ObjectPlace.List => "[color white]",
        ObjectPlace.ListMember => "[color white]",
        ObjectPlace.Enum => "[color #b5cea8]",
        ObjectPlace.Other => "[color white]",
        _ => string.Empty
    };
}
