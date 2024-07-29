using System.IO.Pipes;
using XFEExtension.NetCore.DelegateExtension;
using XFEExtension.NetCore.FormatExtension;
using XFEExtension.NetCore.ProcessCommunication.Client;

namespace XFEExtension.NetCore.ProcessCommunication.Server;

/// <summary>
/// 进程间通讯服务器
/// </summary>
/// <param name="serverName">服务器名称</param>
/// <param name="password">服务器密码</param>
public class ProcessCommunicationServer(string serverName, string password = "")
{
    /// <summary>
    /// 客户端连接事件
    /// </summary>
    public event XFEEventHandler<ProcessCommunicationServer, ProcessCommunicationClientInfo>? ClientConnected;
    /// <summary>
    /// 服务器名称
    /// </summary>
    public string ServerName { get; set; } = serverName;
    /// <summary>
    /// 认证密码
    /// </summary>
    public string Password { get; set; } = password;
    /// <summary>
    /// 管道服务器
    /// </summary>
    public Dictionary<int, NamedPipeServerStream> PipeServerDictionary { get; set; } = [];
    /// <summary>
    /// 启动服务器
    /// </summary>
    /// <param name="autoAuthenticationWithClient">自动对客户端进行认证</param>
    /// <param name="perClientTimeOut">每个客户端的连接超时</param>
    public void Start(bool autoAuthenticationWithClient, int perClientTimeOut = 500)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                var currentServerIndex = PipeServerDictionary.Last().Key + 1;
                using var authenticationPipeServer = new NamedPipeServerStream($"{ServerName}-{currentServerIndex}");
                var tokenSource = new CancellationTokenSource(perClientTimeOut);
                PipeServerDictionary.Add(currentServerIndex, authenticationPipeServer);
                await authenticationPipeServer.WaitForConnectionAsync();
                _ = Task.Run(async () =>
                {
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
                                if (autoAuthenticationWithClient && Password == clientInfo.Password)

                                    ClientConnected?.Invoke(this, clientInfo);

                            }
                        }
                    }
                    else
                    {

                    }
                });
            }
        });
    }
}
