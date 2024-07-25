namespace XFEExtension.NetCore.ProcessCommunication.Client;

/// <summary>
/// 连接结果
/// </summary>
public class ConnectResult
{
    /// <summary>
    /// 成功
    /// </summary>
    public static ConnectResult Successful { get; set; } = new ConnectResult()
    {
        IsSuccessful = true,
        Message = "Successful",
        FailReason = null
    };
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccessful { get; set; }
    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = "";
    /// <summary>
    /// 失败原因
    /// </summary>
    public ConnectFailReason? FailReason { get; set; }
}
