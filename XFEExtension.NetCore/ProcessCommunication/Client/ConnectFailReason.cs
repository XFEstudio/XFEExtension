namespace XFEExtension.NetCore.ProcessCommunication.Client;

/// <summary>
/// 连接失败原因代码
/// </summary>
public enum ConnectFailReason
{
    /// <summary>
    /// 无法连接至认证服务器
    /// </summary>
    FailedToConnectAuthenticationServer = 0,
    /// <summary>
    /// 认证失败
    /// </summary>
    AuthenticationFailed = 1,
    /// <summary>
    /// 服务器自身错误
    /// </summary>
    ServerError = 2,
    /// <summary>
    /// 无法连接至明文服务器
    /// </summary>
    FailedToConnectTextServer = 3,
    /// <summary>
    /// 无法连接至二进制服务器
    /// </summary>
    FailedToConnectBinaryServer = 4,
    /// <summary>
    /// 连接至服务器超时
    /// </summary>
    ConnectToServerTimeOut = 5
}
