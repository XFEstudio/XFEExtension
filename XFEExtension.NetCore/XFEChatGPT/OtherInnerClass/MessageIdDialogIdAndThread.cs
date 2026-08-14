namespace XFEExtension.NetCore.XFEChatGPT.OtherInnerClass;

class MessageIdDialogIdAndThread(string messageId, string dialogId, Thread thread) : MessageIdAndThread(messageId, thread)
{
    /// <summary>
    /// 获取或设置后台处理线程所对应的对话 ID。
    /// </summary>
    public string DialogId { get; set; } = dialogId;
}
