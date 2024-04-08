namespace XFEExtension.NetCore.XFETransform;

internal abstract class SubObjects(List<IObjectInfo>? objectInfoList) : SubObjectsBase(objectInfoList)
{
    public override string OutPutSubObjects()
    {
        var outString = string.Empty;
        for (int i = 0; i < objectInfoList.Count; i++)
        {
            var obj = objectInfoList[i];
            var tabString = string.Empty;
            var currentConnectString = string.Empty;
            for (int k = 0; k < obj.Layer; k++)
            {
                tabString += "│    ";
            }
            outString += tabString;
            if (i == objectInfoList.Count - 1)
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
                outString += obj.OutPut();
            }
            else
            {
                outString += $"""
                     ├─{obj.AddObjectPlace()} {XFEConverter.OutPutTypeName(obj.Type)} {obj.Name}
                     {tabString}├────┬────────────────────────────
                     {obj.OutPut()}
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
}
