using System.Text.Json;
using XFEExtension.NetCore.StringExtension;
using XFEExtension.NetCore.StringExtension.Json;
using XFEExtension.NetCore.XFETransform;

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
        var json = testClass.ToJson();
        //var json = JsonSerializer.Serialize(testClass);
        //testClass.Enum.ToString().CW();
        //Console.WriteLine(json);
        //JsonSerializer.Deserialize<TestClass>(json).X();
        var testString = "this is a t//e/st #//# /# divide : string";
        var escapeConverter = new EscapeConverter("/", "#", ":");
        var convertedString = escapeConverter.Convert(testString);
        Console.WriteLine(convertedString);
        escapeConverter.Inverse(convertedString).CW();
    }
}
enum MyEnum
{
    Test1 = 3,
    Test2 = 12,
    Test3 = 1
}