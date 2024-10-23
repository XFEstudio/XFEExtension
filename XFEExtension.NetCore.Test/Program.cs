using XFEExtension.NetCore.FileExtension;
using XFEExtension.NetCore.XFEChatGPT;
using XFEExtension.NetCore.XFETransform.JsonConverter;

internal class Program
{
    public static MemorableXFEChatGPT XFEChatGPT { get; set; } = new MemorableXFEChatGPT();
    public static string CurrentDialogID { get; set; } = Guid.NewGuid().ToString();
    private static void Main(string[] args)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36 Edg/129.0.0.0");
        client.DefaultRequestHeaders.Add("Origin", "https://www.piyao.org.cn");
        client.DefaultRequestHeaders.Add("Referer", "https://www.piyao.org.cn/");
        QueryableJsonNode jsonNode = @"C:\Users\XFEstudio\Desktop\work\C#\GitHub\XFEstudio\XUnitConsole\XUnitConsole\bin\Debug\net8.0\Json存储.txt".ReadOut()!;
        //foreach (var node in jsonNode["data"]["package:list","name", "avatar_url_template"].PackageInListObject())
        //{
        //    Console.WriteLine($"名称：{node["name"]}\tID：{node["avatar_url_template"]}");
        //}
        foreach (var node in jsonNode["data"].GetChildNodes())
        {
            Console.WriteLine($"名称：{node["name"]}");
        }
    }

    private static void XFEChatGPT_XFEChatGPTMessageReceived(object? sender, XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass.MemorableGPTMessageReceivedEventArgs e)
    {
        switch (e.GenerateState)
        {
            case GenerateState.Start:
                Console.Write(e.Message);
                break;
            case GenerateState.Continue:
                Console.Write(e.Message);
                break;
            case GenerateState.End:
                Console.WriteLine(e.Message);
                break;
            case GenerateState.Error:
                Console.WriteLine(e.Message);
                break;
            default:
                break;
        }
    }
    //[SMTest]
    //[UseXFEConsole]
    //public static async Task TestMethod()
    //{
    //    var testClass = new TestClass("测试类", "测试描述：我上早八", 56)
    //    {
    //        Tags = [["标签11", "标签12"], ["标签21", "标签22"]]
    //    };
    //    Console.WriteLine(testClass);
    //    Console.WriteLine("Program End");
    //    Console.ReadLine();
    //}
}