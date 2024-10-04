using XFEExtension.NetCore.ListExtension;

internal class Program
{
    private static void Main(string[] args)
    {
        var rumorsString = "[+-RumorEntry { TimeLine = 2022/03/21, Content = 东航MU5735客机坠毁事故发生后，网络上流传多条谣言，包括坠机引发山火的视频不实、假冒失联者家属、篡改预言等。 }-+][+-RumorEntry { TimeLine = 2022/03/23, Content = 知情人称东航失事客机机长是“飞二代”，但关于机长或副驾驶故意坠机的谣言被辟谣。 }-+][+-RumorEntry { TimeLine = 2024/05/14, Content = 博主“摆烂兔兔”发布消息称东航MU742次航班因机长失踪导致延误，实则因天气原因备降金边，东航已辟谣。 }-+][+-RumorEntry { TimeLine = 2024/06/07, Content = 关于东航客机事件的谣言称副驾驶因待遇降级报复乘客，官方辟谣并指出谣言干扰调查工作，呼吁不传谣、不信谣。 }-+][+-RumorEntry { TimeLine = 2024/08/15, Content = 东航客机坠毁后，出现多种谣言，包括虚假的事故现场视频、假冒家属发声以及篡改后的预言等。 }-+]";
        rumorsString.ToXFEList<RumorsSearcher.Model.RumorEntry>();
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