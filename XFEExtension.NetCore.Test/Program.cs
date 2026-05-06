using XFEExtension.NetCore.FileExtension;
using XFEExtension.NetCore.XFETransform.JsonConverter;

internal class Program
{
    private static void Main(string[] args)
    {
        QueryableJsonNode jsonNode = @"C:\Users\XFEstudio\Desktop\测试\Bilibili解析失败.txt".ReadOut()!;
        var value = jsonNode["test1"]["subObject"]["testText"];
        value = jsonNode["test1"]?["subObject"]?["testText"] ?? QueryableJsonNode.Empty;
        value = jsonNode > "test1" > "subObject" > "testText";
        value = jsonNode < "test1" < "subObject" < "testText";
        value = jsonNode < "test1" > "subObject" < "testText" > "myProperty";
        //value = jsonNode - "test1" - "subObject" - "testText" - "myProperty";
        //value = jsonNode / "test1" / "subObject" / "testText" / "myProperty";
        //var testClass = new TestClass("123", "sda", 12);
        //testClass.ToJson().CW();
    }
}