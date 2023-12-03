namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

class PrivateMemorableGPTMessageReceivedEventArgs(string message, string id, GenerateState generateState, string dialogId) : MemorableGPTMessageReceivedEventArgs(message, id, generateState, dialogId) { }