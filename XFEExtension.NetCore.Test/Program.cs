using XFEExtension.NetCore.Test;
using XFEExtension.NetCore.XFEConsole;
using XFEExtension.NetCore.XUnit;

internal class Program
{

    private static async Task Main(string[] args)
    {
        await XFECode.RunTest();
    }

    [SMTest]
    [UseXFEConsole]
    public static async Task TestMethod()
    {
        var testClass = new TestClass("测试类", "测试描述：我上早八", 56)
        {
            Tags = [["标签11", "标签12"], ["标签21", "标签22"]]
        };
        Console.WriteLine(testClass);
        Console.WriteLine("Program End");
        Console.ReadLine();
    }
}