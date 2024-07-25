using System.IO.Pipes;
using XFEExtension.NetCore.DelegateExtension;
using XFEExtension.NetCore.FormatExtension;
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
    public Dictionary<int, NamedPipeServerStream> AuthenticationPipeServerDictionary { get; set; } = [];
    public void Start(bool autoAuthenticationWithClient, int perClientTimeOut = 500)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                var currentServerIndex = AuthenticationPipeServerDictionary.Last().Key + 1;
                using var authenticationPipeServer = new NamedPipeServerStream($"{ServerName}-{currentServerIndex}");
                var tokenSource = new CancellationTokenSource(perClientTimeOut);
                AuthenticationPipeServerDictionary.Add(currentServerIndex, authenticationPipeServer);
                await authenticationPipeServer.WaitForConnectionAsync();
                using var streamReader = new StreamReader(authenticationPipeServer);
                using var streamWriter = new StreamWriter(authenticationPipeServer);
                var clientInfoString = await streamReader.ReadLineAsync(tokenSource.Token);
                if (clientInfoString is not null)
                {
                    var clientInfoDictionary = new XFEDictionary(clientInfoString);
                    if (clientInfoDictionary is not null)
                    {
                        if (clientInfoDictionary.Contains("ServerName") && clientInfoDictionary.Contains("ClientName") && clientInfoDictionary.Contains("ClientID") && clientInfoDictionary.Contains("Password"))
                        {
                            var clientInfo = new ProcessCommunicationClientInfo(clientInfoDictionary["ClientName"]!, clientInfoDictionary["ClientID"]!, clientInfoDictionary["Password"]!);
                            ClientConnected?.Invoke(this, clientInfo);
                        }
                    }
                }
            }
        });
    }
}
