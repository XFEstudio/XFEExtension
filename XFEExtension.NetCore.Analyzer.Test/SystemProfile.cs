using XFEExtension.NetCore.ProfileExtension;

namespace XFEExtension.NetCore.Analyzer.Test;

public static partial class SystemProfile
{
    #region Name
    /// <summary>
    /// 名称
    /// </summary>
    [ProfileProperty]
    [ProfilePropertyAddGet(@"Console.WriteLine(""获取了Name"")")]
    [ProfilePropertyAddGet("return name")]
    private static string name = string.Empty;
    #endregion
    [ProfileProperty]
    private static int _age;
}
