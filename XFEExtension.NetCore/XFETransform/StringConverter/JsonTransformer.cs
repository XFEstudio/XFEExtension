using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

namespace XFEExtension.NetCore.XFETransform.StringConverter;

/// <summary>
/// Json转换器
/// </summary>
internal class JsonTransformer : StringConverter
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
            if (IsEnumerable(objectInfo))
            {
                if (objectInfo.Value is null)
                {
                    outPutString += "null";
                }
                else
                {
                    outPutString += objectInfo.Value;
                }
            }
            else
            {
                if (objectInfo.Value is null)
                {
                    outPutString += $"{tabString}\"{objectInfo.Name}\": null";
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
                    {
                    {{OutPutSubObjects(objectInfo.SubObjects)}}
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

    internal override string OutPutSubObjects(ISubObjects subObjects)
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
            outString += tabString;
            string? currentConnectString;
            if (i == subObjects.Count - 1)
            {
                currentConnectString = "\n";
            }
            else
            {
                currentConnectString = ",\n";
            }
            if (obj.IsBasicType)
            {
                outString += obj.OutPutObject();
                outString += currentConnectString;
            }
            else
            {
                if (IsEnumerable(obj))
                    outString += obj is not null ? $$"""
                     [{{OutPutObject(obj)}}]{{currentConnectString}}
                     """ : "null";
                else
                    outString += obj is not null ? $$"""
                     {{tabString}}{
                     {{tabString}}{{OutPutObject(obj)}}
                      {{tabString}}}
                     """ : "null";
            }
        }
        if (outString.Length > 0 && outString[^1] == '\n')
        {
            outString = outString[..^1];
        }
        return outString;
    }

    internal bool IsEnumerable(IObjectInfo objectInfo) => objectInfo.ObjectPlace == ObjectPlace.Array || objectInfo.ObjectPlace == ObjectPlace.List || objectInfo.ObjectPlace == ObjectPlace.ArrayMember || objectInfo.ObjectPlace == ObjectPlace.ListMember;
}