using System.IO.Pipes;
using XFEExtension.NetCore.DelegateExtension;
using XFEExtension.NetCore.ProcessCommunication.Client;

namespace XFEExtension.NetCore.ProcessCommunication.Server;

/// <summary>
/// 进程间通讯认证服务器
/// </summary>
/// <param name="serverName">认证服务器名称</param>
/// <param name="password">认证服务器密码</param>
public class ProcessCommunicationAuthenticationServer(string serverName, string password = "")
{
    /// <summary>
    /// 客户端连接事件
    /// </summary>
    public event XFEEventHandler<ProcessCommunicationAuthenticationServer, ProcessCommunicationClientInfo>? ClientConnected;
    /// <summary>
    /// 认证服务器名称
    /// </summary>
    public string ServerName { get; set; } = serverName;
    /// <summary>
    /// 认证密码
    /// </summary>
    public string Password { get; set; } = password;
    /// <summary>
    /// 认证管道服务器
    /// </summary>
    public NamedPipeServerStream AuthenticationPipeServer { get; set; } = new NamedPipeServerStream(serverName);
    public void Start(bool autoAuthenticationWithClient, int perClientTimeOut = 500)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                await AuthenticationPipeServer.WaitForConnectionAsync();

            }
        });
    }
}
