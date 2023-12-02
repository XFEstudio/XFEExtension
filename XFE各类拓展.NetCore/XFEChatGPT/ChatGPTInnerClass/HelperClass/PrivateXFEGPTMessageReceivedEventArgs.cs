namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass
{
    class PrivateXFEGPTMessageReceivedEventArgs(string message, string id, GenerateState generateState) : XFEGPTMessageReceivedEventArgs(message, id, generateState)
    {
    }
}