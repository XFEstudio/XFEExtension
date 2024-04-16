using System.Diagnostics;
using XFEExtension.NetCore.XUnit;

namespace XFEExtension.NetCore.Analyzer.Test;

internal class Program
{
    public static void Main(string[] args)
    {
        XFECode.RunTest().Wait();
        var process = Process.GetProcessById(13880);
        //process.X();
    }
    [SMTest]
    public static TestClass CreateTestClass()
    {
        var testClass = new TestClass("测试名称", "测试描述哈哈", 59)
        {
            Tags = [["123", "321"], ["1234567", "7654321"]],
            Enum = MyEnum.Test1
        };
        return testClass;
    }
}
enum MyEnum
{
    Test1 = 3,
    Test2 = 12,
    Test3 = 1
}