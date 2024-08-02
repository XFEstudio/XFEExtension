using System.Net.WebSockets;
using XFEExtension.NetCore.CyberComm;
using XFEExtension.NetCore.DelegateExtension;

namespace XFEExtension.NetCore.XFEConsole;

/// <summary>
/// XFE控制台终端服务器
/// </summary>
public class XFEConsoleTerminalServer
{
    /// <summary>
    /// 服务器
    /// </summary>
    public CyberCommServer Server { get; set; }
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }
    /// <summary>
    /// Socket客户端-客户端信息字典
    /// </summary>
    public Dictionary<WebSocket, XFEConsoleClientInfo> ClientInfoDictionary { get; set; } = [];
    /// <summary>
    /// 客户端连接事件
    /// </summary>
    public event XFEEventHandler<XFEConsoleTerminalServer, XFEConsoleClientInfo>? Connected;
    /// <summary>
    /// 客户端断开连接事件
    /// </summary>
    public event XFEEventHandler<XFEConsoleTerminalServer, XFEConsoleClientInfo>? Disconnected;
    /// <summary>
    /// 接收到客户端消息触发
    /// </summary>
    public event XFEEventHandler<XFEConsoleClientInfo, string>? MessageReceived;
    /// <summary>
    /// 发生错误
    /// </summary>
    public event XFEEventHandler<XFEConsoleClientInfo, Exception>? ErrorOccurred;
    /// <summary>
    /// 服务器启动事件
    /// </summary>
    public event XFEEventHandler<XFEConsoleTerminalServer>? ServerStarted;
    /// <summary>
    /// XFE控制台终端服务器
    /// </summary>
    /// <param name="port">端口号</param>
    /// <param name="password">密码（默认为空）</param>
    public XFEConsoleTerminalServer(int port, string password = "")
    {
        Password = password;
        Server = new CyberCommServer(port);
        Server.ServerStarted += Server_ServerStarted;
        Server.ClientConnected += Server_ClientConnected;
        Server.ConnectionClosed += Server_ConnectionClosed;
        Server.MessageReceived += Server_MessageReceived;
    }

    private void Server_MessageReceived(object? sender, CyberCommServerEventArgs e)
    {
        switch (e.MessageType)
        {
            case BackMessageType.Text:
                break;
            case BackMessageType.Binary:
                break;
            case BackMessageType.Error:
                break;
            default:
                break;
        }
    }

    private void Server_ConnectionClosed(object? sender, CyberCommServerEventArgs e)
    {
        if (ClientInfoDictionary.TryGetValue(e.CurrentWebSocket, out XFEConsoleClientInfo? clientInfo))
        {
            ClientInfoDictionary.Remove(e.CurrentWebSocket);
            Disconnected?.Invoke(this, clientInfo);
        }
    }

    private async void Server_ClientConnected(object? sender, CyberCommServerEventArgs e)
    {
        try
        {
            if (e.WSHeader["ClientName"] is not null && e.WSHeader["ClientID"] is not null && e.WSHeader["Password"] is not null)
            {
                var password = e.WSHeader["Password"]!;
                var clientName = e.WSHeader["ClientName"]!;
                var clientUUID = e.WSHeader["ClientID"]!;
                if (password == Password)
                {
                    var clientInfo = new XFEConsoleClientInfo(clientName, clientUUID, password, e);
                    ClientInfoDictionary.Add(e.CurrentWebSocket, clientInfo);
                    Connected?.Invoke(this, clientInfo);
                }
            }
        }
        catch { }
        try
        {
            await e.Close();
        }
        catch
        {
            try
            {
                e.ForceClose();
            }
            catch { }
        }
    }

    private void Server_ServerStarted(object? sender, EventArgs e)
    {
        ClientInfoDictionary = [];
    }
    /// <summary>
    /// 启动服务器
    /// </summary>
    /// <returns></returns>
    public async Task StartServer()
    {
        await Server.StartCyberCommServer();
    }
}
