using System.Net.WebSockets;

namespace XFE各类拓展.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// XCC连接事件
/// </summary>
public abstract class XCCConnectedEventArgs : EventArgs
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

    internal XCCConnectedEventArgs(XCCGroup group, XCCClientType xCCClientType, ClientWebSocket? textMessageClientWebSocket, ClientWebSocket? fileTransportClientWebSocket)
    {
        Group = group;
        XCCClientType = xCCClientType;
        TextMessageClientWebSocket = textMessageClientWebSocket;
        FileTransportClientWebSocket = fileTransportClientWebSocket;
    }
}
