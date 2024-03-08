using XFEExtension.NetCore.ProfileExtension;

namespace XFEExtension.NetCore.Analyzer.Test;

public static partial class SystemProfile
{
    #region Name
    /// <summary>
    /// 名称
    /// </summary>
    [ProfileProperty]
    [ProfilePropertyAddGet("Console.WriteLine()")]
    [ProfilePropertyAddGet("Console.WriteLine()")]
    [ProfilePropertyAddGet($@"return name")]
    [ProfilePropertyAddSet("Console.WriteLine(value)")]
    [ProfilePropertyAddSet("name = value")]
    private static string name = string.Empty;
    #endregion
    [ProfileProperty]
    private static int _age;
}
