using XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;

namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

class PrivateReceivedGPTMessage(string id, string @object, long created, string model, TokenUsage usage, PrivateMessageChoice[] choices) : ReceivedGPTMessage(id, @object, created, model, usage, choices) { }