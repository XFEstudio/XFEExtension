namespace XFE各类拓展.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// XFE网络通信二进制返回消息类型
/// </summary>
public enum XCCBinaryMessageType
{
    /// <summary>
    /// 文本消息
    /// </summary>
    Text,
    /// <summary>
    /// 二进制消息
    /// </summary>
    Binary,
    /// <summary>
    /// 图片消息
    /// </summary>
    Image,
    /// <summary>
    /// 音频消息
    /// </summary>
    Audio,
    /// <summary>
    /// 实时音频
    /// </summary>
    AudioBuffer,
    /// <summary>
    /// 视频消息
    /// </summary>
    Video
}
