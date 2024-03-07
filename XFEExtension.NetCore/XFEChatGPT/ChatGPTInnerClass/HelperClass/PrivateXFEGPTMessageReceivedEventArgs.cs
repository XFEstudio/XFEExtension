namespace XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

class PrivateXFEGPTMessageReceivedEventArgs(string message, string id, GenerateState generateState) : XFEGPTMessageReceivedEventArgs(message, id, generateState) { }