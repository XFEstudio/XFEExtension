using System.Net.WebSockets;

namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// XCC会话关闭事件
/// </summary>
public abstract class XCCConnectionClosedEventArgs : EventArgs
{
    /// <summary>
    /// 触发事件的群组
    /// </summary>
    public XCCGroup Group { get; }
    /// <summary>
    /// XCC服务器连接类型
    /// </summary>
    public XCCClientType XCCClientType { get; }
    /// <summary>
    /// WebSocket明文传输客户端
    /// </summary>
    public ClientWebSocket? TextMessageClientWebSocket { get; private set; }
    /// <summary>
    /// WebSocket文件传输客户端
    /// </summary>
    public ClientWebSocket? FileTransportClientWebSocket { get; private set; }
    /// <summary>
    /// 是否正常关闭
    /// </summary>
    public bool ClosedNormally { get; }

    internal XCCConnectionClosedEventArgs(XCCGroup group, XCCClientType xCCClientType, ClientWebSocket? textMessageClientWebSocket, ClientWebSocket? fileTransportClientWebSocket, bool closedNormally)
    {
        Group = group;
        XCCClientType = xCCClientType;
        TextMessageClientWebSocket = textMessageClientWebSocket;
        FileTransportClientWebSocket = fileTransportClientWebSocket;
        ClosedNormally = closedNormally;
    }
}
