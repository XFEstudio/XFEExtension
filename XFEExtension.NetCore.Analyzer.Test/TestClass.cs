namespace XFEExtension.NetCore.Analyzer.Test;

internal class TestClass
{
    static void Main(string[] args)
    {
        var task = new Task(() =>
        {
            Console.WriteLine("Hello World");
        });
    }
}