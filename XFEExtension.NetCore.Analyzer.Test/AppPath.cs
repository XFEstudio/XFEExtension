using XFEExtension.NetCore.PathExtension;

namespace XFEExtension.NetCore.Analyzer.Test;

public partial class AppPath
{
    [AutoPath]
    readonly static string myTestPath = "MyTestPath/Test";
    [AutoPath]
    readonly static string mySecTestPath = $"{MyTestPath}/Sec";
}
