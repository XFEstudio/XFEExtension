using XFEExtension.NetCore.FileExtension.XFEPackage;

internal class Program
{
    private static void Main(string[] args)
    {
        XFEPackage.PackFile(@"C:\Users\XFEstudio\AppData\LocalLow\Element Games\Reality Break", "测试.package");
        XFEPackage.UnPackFile("测试.package", "测试");
    }
}