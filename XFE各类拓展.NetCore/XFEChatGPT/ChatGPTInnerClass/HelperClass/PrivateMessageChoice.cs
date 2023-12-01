using XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;

namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass
{
    class PrivateMessageChoice : MessageChoice
    {
        public PrivateMessageChoice(GPTMessage delta, GPTMessage message, string finish_reason, int index) : base(delta, message, finish_reason, index) { }
        public PrivateMessageChoice(GPTMessage message, string finish_reason, int index) : base(message, finish_reason, index) { }
    }
}