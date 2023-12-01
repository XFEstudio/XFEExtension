namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass
{
    class PrivateXFEGPTMessageReceivedEventArgs : XFEGPTMessageReceivedEventArgs
    {
        public PrivateXFEGPTMessageReceivedEventArgs(string message, string id, GenerateState generateState) : base(message, id, generateState)
        {

        }
    }
}