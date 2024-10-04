using XFEExtension.NetCore.XFEChatGPT;

internal class Program
{
    public static MemorableXFEChatGPT XFEChatGPT { get; set; } = new MemorableXFEChatGPT();
    public static string CurrentDialogID { get; set; } = Guid.NewGuid().ToString();
    private static async Task Main(string[] args)
    {
        XFEChatGPT.XFEChatGPTMessageReceived += XFEChatGPT_XFEChatGPTMessageReceived;
        XFEChatGPT.CreateDialog(CurrentDialogID, "你是一个专门负责整理谣言的人工智能，我会提供给你一些带有时间线的谣言，这其中可能会有很多不相干的信息和没有时间线的信息，你需要自己辨别判断有用信息并将其排序从最早的到最新的日期来排序，除此之外不要说任何多余的内容，格式是：[+-RumorEntry { TimeLine = 2017/2/4, Content = 你整理的第一条谣言内容 }-+][+-RumorEntry { TimeLine = 2019/8/12, Content = 你整理的第二条谣言内容 }-+][+-RumorEntry { TimeLine = 2022/10/4, Content = 你整理的第三条谣言内容 }-+][+-RumorEntry { TimeLine = 2023/1/23, Content = 你整理的第四条谣言内容 }-+]...以此类推", true, false, ChatGPTModel.gpt4o);
        //await XFEConsole.WriteObject(XFEChatGPT, false, false);
        //Console.WriteLine(XFEChatGPT);
        await XFEChatGPT.AskChatGPT(CurrentDialogID, Guid.NewGuid().ToString(), "测试谣言");
        Console.ReadLine();
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