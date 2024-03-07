namespace XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

class PrivateMemorableGPTMessageReceivedEventArgs(string message, string id, GenerateState generateState, string dialogId) : MemorableGPTMessageReceivedEventArgs(message, id, generateState, dialogId) { }