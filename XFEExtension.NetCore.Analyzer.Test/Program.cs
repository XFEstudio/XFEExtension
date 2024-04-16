using System.Diagnostics;
using XFEExtension.NetCore.FileExtension;
using XFEExtension.NetCore.StringExtension;

namespace XFEExtension.NetCore.Analyzer.Test;

internal class Program
{
    public static void Main(string[] args)
    {
        var testClass = new TestClass("测试名称", "测试描述哈哈", 59)
        {
            Tags = [["123", "321"], ["1234567", "7654321"]],
            Enum = MyEnum.Test1
        };
        var process = Process.GetProcessesByName("QQ");
        process.X()?.WriteIn("QQAnalyze.txt");
        testClass.X()?.WriteIn("TestClassAnalyze.txt");
    }
}
enum MyEnum
{
    Test1 = 3,
    Test2 = 12,
    Test3 = 1
}