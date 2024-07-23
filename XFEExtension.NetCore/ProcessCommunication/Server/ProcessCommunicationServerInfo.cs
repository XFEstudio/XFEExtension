namespace XFEExtension.NetCore.ProcessCommunication.Server;

/// <summary>
/// 进程通讯服务器信息
/// </summary>
public class ProcessCommunicationServerInfo(string serverName, string currentTextServerPipeName, string currentBinaryServerPipeName)
{
    /// <summary>
    /// 服务器名称
    /// </summary>
    public string ServerName { get; set; } = serverName;
    /// <summary>
    /// 当前明文服务器管道名称
    /// </summary>
    public string CurrentTextServerPipeName { get; set; } = currentTextServerPipeName;
    /// <summary>
    /// 当前二进制服务器管道名称
    /// </summary>
    public string CurrentBinaryServerPipeName { get; set; } = currentBinaryServerPipeName;
}
