using System.IO.Pipes;
using XFEExtension.NetCore.ProcessCommunication.Client;

namespace XFEExtension.NetCore.ProcessCommunication.Server;

/// <summary>
/// 进程间通讯服务器
/// </summary>
public class ProcessCommunicationServer
{
    /// <summary>
    /// 管道通讯服务器
    /// </summary>
    public NamedPipeServerStream PipeServer { get; set; }
    /// <summary>
    /// 是否有客户端连接
    /// </summary>
    public bool HasClient { get => ClientInfo is not null; }
    /// <summary>
    /// 客户端名称
    /// </summary>
    public string? ClientName { get; set; }
    /// <summary>
    /// 客户端信息
    /// </summary>
    public ProcessCommunicationClientInfo? ClientInfo { get; set; }
    public ProcessCommunicationServer(NamedPipeServerStream pipeServer)
    {
        PipeServer = pipeServer;
    }
}
