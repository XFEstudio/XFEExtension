using XFEExtension.NetCore.FileExtension;
using XFEExtension.NetCore.XFETransform.JsonConverter;

internal class Program
{
    private static void Main(string[] args)
    {
        QueryableJsonNode jsonNode = @"C:\Users\XFEstudio\Desktop\测试\Json数组解析失败.txt".ReadOut()!;
    }
} 