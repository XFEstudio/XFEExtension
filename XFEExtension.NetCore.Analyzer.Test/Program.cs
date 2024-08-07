namespace XFEExtension.NetCore.Analyzer.Test;

internal class Program
{
    [SMTest]
    public static void TestNamedPipeAsync()
    {
        Console.WriteLine("Hello World!");
        Console.WriteLine("Program End");
    }
}
enum MyEnum
{
    Test1 = 3,
    Test2 = 12,
    Test3 = 1
}