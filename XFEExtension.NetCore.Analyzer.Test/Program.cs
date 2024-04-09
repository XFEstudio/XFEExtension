using XFEExtension.NetCore.StringExtension;
using XFEExtension.NetCore.XFETransform;
using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;

namespace XFEExtension.NetCore.Analyzer.Test;

internal class Program
{
    public static void Main(string[] args)
    {
        var testClass = new TestClass("测试名称", "测试描述哈哈", 59)
        {
            Tags = [["123", "321"], ["1234567", "7654321"]]
        };
        XFEConverter.GetObjectInfo(new JsonTransformer(), "Json序列化对象", ObjectPlace.Main, 0, typeof(TestClass), testClass).OutPutObject().CW();
    }
}