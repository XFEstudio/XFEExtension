using XFEExtension.NetCore.StringExtension;

namespace XFEExtension.NetCore.Analyzer.Test;

internal class Program
{
    public static void Main(string[] args)
    {
        var testClass = new TestClass("测试名称", "测试描述内容123123123", 60)
        {
            Tags = [["123", "321"], ["231", "132"]]
        };
        testClass.X();
    }
}