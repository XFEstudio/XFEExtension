using XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;

namespace XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

class XFEAskGPTMessage
{
    /// <summary>
    /// 获取或设置是否直接使用调用方提供的 GPT 环境数据。
    /// </summary>
    public bool IsSelfEditData { get; set; }
    /// <summary>
    /// 获取或设置是否以流式方式接收 GPT 响应。
    /// </summary>
    public bool Stream { get; set; }
    /// <summary>
    /// 获取或设置请求使用的 GPT 模型名称。
    /// </summary>
    public string? ChatGPTModel { get; set; }
    /// <summary>
    /// 获取或设置调用方自定义的 GPT 环境数据。
    /// </summary>
    public EnvironmentGPTData? EnvironmentGPTData { get; set; }
    /// <summary>
    /// 获取或设置请求使用的 XFE 通讯协议。
    /// </summary>
    public XFEComProtocol ComProtocol { get; set; }
    /// <summary>
    /// 获取或设置用于定义 GPT 行为的系统消息内容。
    /// </summary>
    public string? SystemContent { get; set; }
    /// <summary>
    /// 获取或设置发送给 GPT 的提问内容。
    /// </summary>
    public string? AskContent { get; set; }
    /// <summary>
    /// 使用请求模式、模型、环境数据及消息内容创建 GPT 请求消息。
    /// </summary>
    /// <param name="isSelfEditData">是否直接使用调用方提供的 GPT 环境数据。</param>
    /// <param name="stream">是否以流式方式接收 GPT 响应。</param>
    /// <param name="chatGPTModel">请求使用的 GPT 模型名称。</param>
    /// <param name="environmentGPTData">调用方自定义的 GPT 环境数据。</param>
    /// <param name="comProtocol">请求使用的 XFE 通讯协议。</param>
    /// <param name="systemContent">用于定义 GPT 行为的系统消息内容。</param>
    /// <param name="askContent">发送给 GPT 的提问内容。</param>
    public XFEAskGPTMessage(bool isSelfEditData, bool stream, string chatGPTModel, EnvironmentGPTData? environmentGPTData, XFEComProtocol comProtocol, string systemContent, string askContent)
    {
        IsSelfEditData = isSelfEditData;
        Stream = stream;
        ChatGPTModel = chatGPTModel;
        EnvironmentGPTData = environmentGPTData;
        ComProtocol = comProtocol;
        SystemContent = systemContent;
        AskContent = askContent;
    }
    /// <summary>
    /// 创建一个使用默认属性值的空 GPT 请求消息。
    /// </summary>
    public XFEAskGPTMessage() { }
}
