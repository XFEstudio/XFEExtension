using System.Formats.Asn1;
using XFEExtension.NetCore.FileExtension;
using XFEExtension.NetCore.StringExtension;
using XFEExtension.NetCore.StringExtension.Json;
using XFEExtension.NetCore.Test;
using XFEExtension.NetCore.XFETransform.JsonConverter;

internal class Program
{
    private static void Main(string[] args)
    {
        //QueryableJsonNode jsonNode = @"C:\Users\XFEstudio\Desktop\测试\Json数组解析失败.txt".ReadOut()!;
        var testClass = new TestClass("123", "sda", 12);
        testClass.ToJson().CW();
    }
}