namespace XFEExtension.NetCore.ProcessCommunication.Client;

/// <summary>
/// 进程通讯客户端信息
/// </summary>
/// <param name="clientName">客户端名称</param>
/// <param name="clientId">客户端唯一标识</param>
/// <param name="password">客户端连接时密码</param>
public class ProcessCommunicationClientInfo(string clientName, string clientId, string password = "")
{
    /// <summary>
    /// 客户端名称
    /// </summary>
    public string ClientName { get; set; } = clientName;
    /// <summary>
    /// 客户端唯一标识
    /// </summary>
    public string ClientID { get; set; } = clientId;
    /// <summary>
    /// 客户端连接时密码
    /// </summary>
    public string Password { get; set; } = password;
}
