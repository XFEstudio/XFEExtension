using XFE各类拓展.NetCore.DelegateExtension;

namespace XFE各类拓展.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// XCC文件
/// </summary>
/// <param name="groupId">群组ID</param>
/// <param name="messageId">文件消息ID</param>
/// <param name="fileType">文件类型</param>
/// <param name="sender">发送者</param>
/// <param name="sendTime">发送时间</param>
/// <param name="fileBuffer">文件的Buffer</param>
public class XCCFile(string groupId, string messageId, XCCFileType fileType, string sender, DateTime? sendTime, byte[]? fileBuffer = null)
{
    /// <summary>
    /// 文件加载完成时触发
    /// </summary>
    public event XFEEventHandler<XCCFile>? FileLoaded;
    /// <summary>
    /// 群组ID
    /// </summary>
    public string GroupId { get; } = groupId;
    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; } = messageId;
    /// <summary>
    /// 发送者
    /// </summary>
    public string Sender { get; } = sender;
    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime? SendTime { get; } = sendTime;
    /// <summary>
    /// XCC文件类型
    /// </summary>
    public XCCFileType FileType { get; } = fileType;
    /// <summary>
    /// 是否已加载
    /// </summary>
    public bool Loaded { get; private set; } = fileBuffer is not null;
    /// <summary>
    /// 文件流
    /// </summary>
    public byte[]? FileBuffer { get; set; } = fileBuffer;
    /// <summary>
    /// 加载文件
    /// </summary>
    /// <param name="fileBuffer">文件流</param>
    public void LoadFile(byte[] fileBuffer)
    {
        FileBuffer = fileBuffer;
        Loaded = true;
        FileLoaded?.Invoke(this);
    }
}
