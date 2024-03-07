namespace XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;
    /// <summary>
    /// GPT环境数据
    /// </summary>
    /// <param name="model"></param>
    /// <param name="stream"></param>
    /// <param name="temperature"></param>
    /// <param name="messages"></param>
    public class EnvironmentGPTData(string model, bool stream, double temperature, GPTMessage[] messages)
{
    /// <summary>
    /// 使用的GPT模型
    /// </summary>
    public string Model { get; set; } = model;
    /// <summary>
    /// 是否为流式接收
    /// </summary>
    public bool Stream { get; set; } = stream;
    /// <summary>
    /// 温度，此处为GPT的创造性
    /// </summary>
    public double Temperature { get; set; } = temperature;
    /// <summary>
    /// 发送给GPT的消息
    /// </summary>
    public GPTMessage[] Messages { get; set; } = messages;
}
