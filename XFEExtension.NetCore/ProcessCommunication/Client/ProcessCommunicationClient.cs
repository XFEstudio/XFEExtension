using System.Diagnostics;
using System.IO.Pipes;
using XFEExtension.NetCore.FormatExtension;
using XFEExtension.NetCore.ProcessCommunication.Server;

namespace XFEExtension.NetCore.ProcessCommunication.Client;

/// <summary>
/// 进程通讯客户端
/// </summary>
public class ProcessCommunicationClient
{
    private StreamReader? streamReader;
    private StreamWriter? streamWriter;
    /// <summary>
    /// 明文管道通讯客户端
    /// </summary>
    public NamedPipeClientStream? TextPipeClient { get; set; }
    /// <summary>
    /// 二进制管道通讯客户端
    /// </summary>
    public NamedPipeClientStream? BinaryPipeClient { get; set; }
    /// <summary>
    /// 客户端名称
    /// </summary>
    public required string ClientName { get; set; }
    /// <summary>
    /// 客户端唯一标识
    /// </summary>
    public required string ClientID { get; set; }
    /// <summary>
    /// 是否已经连接到服务器
    /// </summary>
    public bool Connected { get; set; }
    /// <summary>
    /// 服务器信息
    /// </summary>
    public ProcessCommunicationServerInfo? ServerInfo { get; set; }
    /// <summary>
    /// 连接
    /// </summary>
    /// <param name="serverName">服务器名称</param>
    /// <param name="password">密码</param>
    /// <param name="computerName">目标计算机名称</param>
    /// <param name="timeOut">超时(ms)</param>
    /// <returns>连接结果</returns>
    public async Task<ConnectResult> Connect(string serverName, string password = "", string computerName = ".", int timeOut = 500)
    {
        var stopWatch = new Stopwatch();
        var tokenSource = new CancellationTokenSource(timeOut);
        int retryTime = 0;
        stopWatch.Start();
        var authenticationClient = new NamedPipeClientStream(computerName, $"{serverName}-{retryTime}");
        while (true)
        {
            try
            {
                await authenticationClient.ConnectAsync(timeOut);
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (stopWatch.ElapsedMilliseconds < timeOut)
                {
                    retryTime++;
                    await authenticationClient.DisposeAsync();
                    authenticationClient = new NamedPipeClientStream($"{serverName}-{retryTime}");
                }
                else
                {
                    return new ConnectResult()
                    {
                        IsSuccessful = false,
                        FailReason = ConnectFailReason.ConnectToServerTimeOut,
                        Message = "连接至认证服务器超时"
                    };
                }
            }
        }
        using var reader = new StreamReader(authenticationClient);
        using var writer = new StreamWriter(authenticationClient);
        await writer.WriteLineAsync(new XFEDictionary("ServerName", serverName, "ClientName", ClientName, "ClientID", ClientID, "Password", password));
        await writer.FlushAsync();
        var result = await reader.ReadLineAsync(tokenSource.Token);
        await authenticationClient.DisposeAsync();
        if (result is not null)
        {
            var resultDictionary = new XFEDictionary(result);
            if (resultDictionary.Contains("ServerName") && resultDictionary.Contains("TextPipeID") && resultDictionary.Contains("BinaryPipeID"))
            {
                ServerInfo = new ProcessCommunicationServerInfo(resultDictionary["ServerName"]!, resultDictionary["TextPipeID"]!, resultDictionary["BinaryPipeID"]!);
                TextPipeClient = new NamedPipeClientStream(computerName, ServerInfo.CurrentTextServerPipeName);
                BinaryPipeClient = new NamedPipeClientStream(computerName, ServerInfo.CurrentBinaryServerPipeName);
                await TextPipeClient.ConnectAsync(tokenSource.Token);
                await BinaryPipeClient.ConnectAsync(tokenSource.Token);
                streamReader = new StreamReader(TextPipeClient);
                streamWriter = new StreamWriter(TextPipeClient);
                return ConnectResult.Successful;
            }
        }
        return ConnectResult.Successful;
    }
    /// <summary>
    /// 发送明文信息
    /// </summary>
    /// <param name="message">明文信息</param>
    /// <param name="timeOut">超时(ms)</param>
    /// <returns>是否发送成功</returns>
    public async Task<bool> SendTextMessage(string message, int timeOut = 50)
    {
        if (!CheckPipeClientExist())
            return false;
        var tokenSource = new CancellationTokenSource(timeOut);
        await streamWriter!.WriteLineAsync(message);
        await streamWriter!.FlushAsync(tokenSource.Token);
        var result = await streamReader!.ReadLineAsync(tokenSource.Token);
        if (result is not null)
        {
            var resultDictionary = new XFEDictionary(result);
            if (resultDictionary.Contains("Successful"))
            {
                return resultDictionary["Successful"] == "true";
            }
        }
        return false;
    }
    /// <summary>
    /// 发送二进制消息
    /// </summary>
    /// <param name="bytes">二进制消息</param>
    /// <param name="timeOut">超时(ms)</param>
    /// <returns></returns>
    public async Task<bool> SendBinaryMessage(byte[] bytes, int timeOut = 50)
    {
        if (!CheckPipeClientExist())
            return false;
        var tokenSource = new CancellationTokenSource(timeOut);
        await BinaryPipeClient!.WriteAsync(bytes, tokenSource.Token);
        await BinaryPipeClient!.FlushAsync(tokenSource.Token);
        var result = new byte[1];
        var resultCount = await BinaryPipeClient!.ReadAsync(result, tokenSource.Token);
        if (resultCount > 0)
        {
            return result[0] == 1;
        }
        return false;
    }
    private bool CheckPipeClientExist() => TextPipeClient is not null && BinaryPipeClient is not null && streamReader is not null && streamWriter is not null;
}
