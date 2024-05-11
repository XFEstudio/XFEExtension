using XFEExtension.NetCore.PathExtension;

namespace XFEExtension.NetCore.Analyzer.Test;

public partial class AppPath
{
    [AutoPath]
    readonly string myTestPath = "MyTestPath/Test";
}
