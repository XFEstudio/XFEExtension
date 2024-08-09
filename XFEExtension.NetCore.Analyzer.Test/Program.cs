using XFEExtension.NetCore.StringExtension;
using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;
using XFEExtension.NetCore.XFETransform;
using XFEExtension.NetCore.XFETransform.StringConverter;

namespace XFEExtension.NetCore.Analyzer.Test;

internal class Program
{
    [SMTest]
    public static async void TestNamedPipeAsync()
    {
        var testClass = new TestClass("测试类", "测试描述：我上早八", 56)
        {
            Tags = [["标签11", "标签12"], ["标签21", "标签22"]],
            Enum = MyEnum.Test2,
            MultiEnum = MyMultiEnum.Get | MyMultiEnum.Pull | MyMultiEnum.Set | MyMultiEnum.Post
        }.GetType();
        var result = XFEConverter.GetObjectInfo(StringConverter.ColoredObjectAnalyzer, "分析对象", ObjectPlace.Main, 0, [testClass], testClass?.GetType(), testClass, false, false).OutPutObject();
        Console.WriteLine(result);
        //testClass.X();
    }
}
enum MyEnum
{
    Test1 = 3,
    Test2 = 12,
    Test3 = 1
}
[Flags]
enum MyMultiEnum
{
    Get = 1,
    Set = 2,
    Post = 4,
    Pull = 8
}