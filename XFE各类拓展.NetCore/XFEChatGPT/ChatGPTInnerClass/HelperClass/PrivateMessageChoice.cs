using XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;

namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

class PrivateMessageChoice : MessageChoice
{
    public PrivateMessageChoice(GPTMessage delta, GPTMessage message, string finishReason, int index) : base(delta, message, finishReason, index) { }
    public PrivateMessageChoice(GPTMessage message, string finishReason, int index) : base(message, finishReason, index) { }
}