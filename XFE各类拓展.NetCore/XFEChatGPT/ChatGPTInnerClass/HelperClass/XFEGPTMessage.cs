using XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;

namespace XFE各类拓展.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass
{
    /// <summary>
    /// XFEGPT消息（具有消息ID）
    /// </summary>
    public class XFEGPTMessage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string messageId { get; set; }
        /// <summary>
        /// GPT消息
        /// </summary>
        public GPTMessage gPTMessage { get; set; }
        /// <summary>
        /// XFEGPT消息（具有消息ID）
        /// </summary>
        /// <param name="messageId">消息ID</param>
        /// <param name="gPTMessage">GPT消息</param>
        public XFEGPTMessage(string messageId, GPTMessage gPTMessage)
        {
            this.messageId = messageId;
            this.gPTMessage = gPTMessage;
        }
    }
}