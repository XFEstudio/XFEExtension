using XFE各类拓展.NetCore.ProfileExtension;

namespace XFE各类拓展.NetCore.Analyzer.Test;

public static partial class SystemProfile
{
    [ProfileProperty]
    [ProfilePropertyAddGet(@"Console.WriteLine(""123"")")]
    [ProfilePropertyAddSet(@"Console.WriteLine(value)")]
    [ProfilePropertyAddGet(@"Console.WriteLine(""456"")")]
    [ProfilePropertyAddSet(@"Console.WriteLine()")]
    private static string name = string.Empty;
    [ProfileProperty]
    [ProfilePropertyAddSet(@"_age = 1")]
    [ProfilePropertyAddSet(@"_age = 2")]
    [ProfilePropertyAddSet(@"_age = value")]
    private static int _age;
}
