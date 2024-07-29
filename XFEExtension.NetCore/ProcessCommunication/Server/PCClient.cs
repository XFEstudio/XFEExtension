using System.IO.Pipes;
using XFEExtension.NetCore.ProcessCommunication.Client;

namespace XFEExtension.NetCore.ProcessCommunication.Server;

/// <summary>
/// 进程间通讯客户端实例
/// </summary>
/// <param name="clientInfo">客户端信息</param>
/// <param name="serverPipe">服务器管道</param>
/// <param name="isConnected">客户端是否连接</param>
public class PCClient(ProcessCommunicationClientInfo clientInfo, NamedPipeServerStream serverPipe, bool isConnected)
{
    /// <summary>
    /// 客户端信息
    /// </summary>
    public ProcessCommunicationClientInfo ClientInfo { get; set; } = clientInfo;
    /// <summary>
    /// 服务器通讯管道
    /// </summary>
    public NamedPipeServerStream ServerPipe { get; set; } = serverPipe;
    /// <summary>
    /// 客户端是否已经连接
    /// </summary>
    public bool IsConnected { get; set; } = isConnected;
}
