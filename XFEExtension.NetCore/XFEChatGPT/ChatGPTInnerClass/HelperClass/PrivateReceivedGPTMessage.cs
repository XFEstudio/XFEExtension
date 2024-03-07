using XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;

namespace XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

class PrivateReceivedGPTMessage(string id, string @object, long created, string model, TokenUsage usage, PrivateMessageChoice[] choices) : ReceivedGPTMessage(id, @object, created, model, usage, choices) { }