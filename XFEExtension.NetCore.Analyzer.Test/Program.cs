using XFEExtension.NetCore.XFETransform.ObjectInfoAnalyzer;
using XFEExtension.NetCore.XFETransform;
using XFEExtension.NetCore.XFETransform.StringConverter;

namespace XFEExtension.NetCore.Analyzer.Test;

internal class Program
{
    /// <summary>
    /// 运行对象信息分析器的控制台测试，并将分析后的对象描述输出到控制台。
    /// </summary>
    public static void Main()
    {
        var testClass = new TestClass("测试类", "测试描述：我上早八", 56)
        {
            Tags = [["标签11", "标签12"], ["标签21", "标签22"]],
            Enum = MyEnum.Test2,
            MultiEnum = MyMultiEnum.Get | MyMultiEnum.Pull | MyMultiEnum.Set | MyMultiEnum.Post
        }.GetType();
        var result = XFEConverter.GetObjectInfo(StringConverter.ColoredObjectAnalyzer, "分析对象", ObjectPlace.Main, 0, [testClass], testClass, testClass, false, false).OutPutObject();
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
