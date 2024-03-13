namespace XFEExtension.NetCore.XFETransform;

internal abstract class SubObjects : SubObjectsBase
{
    public override string OutPutSubObjects()
    {
        var outString = string.Empty;
        for (int i = 0; i < objectInfoList.Count; i++)
        {
            var obj = objectInfoList[i];
            for (int j = 0; j < obj.Layer; j++)
            {
                outString += "│    ";
            }
            if (i == objectInfoList.Count - 1)
            {
                outString += "└─";
            }
            else
            {
                outString += "├─";
            }
            outString += obj.OutPut();
        }
        if (outString.Length > 0 && outString[^1] == '\n')
        {
            outString = outString[..^1];
        }
        return outString;
    }
}
