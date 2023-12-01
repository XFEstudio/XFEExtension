using XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;

namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass
{
    class XFEAskGPTMessage
    {
        public bool isSelfEditData { get; set; }
        public bool stream { get; set; }
        public string chatGPTModel { get; set; }
        public EnvironmentGPTData EnvironmentGPTData { get; set; }
        public XFEComProtocol comProtocol { get; set; }
        public string systemContent { get; set; }
        public string askContent { get; set; }
        public XFEAskGPTMessage(bool isSelfEditData, bool stream, string chatGPTModel, EnvironmentGPTData EnvironmentGPTData, XFEComProtocol comProtocol, string systemContent, string askContent)
        {
            this.isSelfEditData = isSelfEditData;
            this.stream = stream;
            this.chatGPTModel = chatGPTModel;
            this.EnvironmentGPTData = EnvironmentGPTData;
            this.comProtocol = comProtocol;
            this.systemContent = systemContent;
            this.askContent = askContent;
        }
        public XFEAskGPTMessage() { }
    }
}