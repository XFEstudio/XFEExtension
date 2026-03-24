using XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;

namespace XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

class XFEAskGPTMessage
{
    public bool IsSelfEditData { get; set; }
    public bool Stream { get; set; }
    public string? ChatGPTModel { get; set; }
    public EnvironmentGPTData? EnvironmentGPTData { get; set; }
    public XFEComProtocol ComProtocol { get; set; }
    public string? SystemContent { get; set; }
    public string? AskContent { get; set; }
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
    public XFEAskGPTMessage() { }
}