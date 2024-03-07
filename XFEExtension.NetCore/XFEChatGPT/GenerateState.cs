namespace XFEExtension.NetCore.XFEChatGPT;

/// <summary>
/// 文本生成状态
/// </summary>
public enum GenerateState
{
    /// <summary>
    /// 表示生成已经开始
    /// </summary>
    Start,
    /// <summary>
    /// 表示生成正在进行中
    /// </summary>
    Continue,
    /// <summary>
    /// 表示生成已经结束
    /// </summary>
    End,
    /// <summary>
    /// 表示生成出错
    /// </summary>
    Error
}