using XFEExtension.NetCore.CyberComm;
using XFEExtension.NetCore.FormatExtension;
using XFEExtension.NetCore.TaskExtension;

namespace XFEExtension.NetCore.XFEConsole;

/// <summary>
/// XFE控制台程序客户端
/// </summary>
/// <remarks>
/// 默认构造函数
/// </remarks>
/// <param name="ipAddress">控制台输出端IP地址</param>
/// <param name="clientName">客户端名称</param>
/// <param name="password">密码</param>
/// <param name="clientID">客户端ID</param>
public class XFEConsoleProgramClient(string ipAddress, string clientName, string password, string clientID)
{
    private event EndTaskTrigger<bool>? ConnectTrigger;
    /// <summary>
    /// 客户端
    /// </summary>
    public CyberCommClient Client { get; set; } = new CyberCommClient(ipAddress);
    /// <summary>
    /// 客户端名称
    /// </summary>
    public string ClientName { get; set; } = clientName;
    /// <summary>
    /// 连接时密码
    /// </summary>
    public string Password { get; set; } = password;
    /// <summary>
    /// 客户端ID
    /// </summary>
    public string ClientID { get; set; } = clientID;
    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Connect()
    {
        Client.Connected += Client_Connected;
        _ = Client.StartCyberCommClient();
        Client.ClientWebSocket!.Options.SetRequestHeader(nameof(ClientName), ClientName);
        Client.ClientWebSocket!.Options.SetRequestHeader(nameof(Password), Password);
        Client.ClientWebSocket!.Options.SetRequestHeader(nameof(ClientID), ClientID);
        if (Client.IsConnected)
            return true;
        else
            return await new XFEWaitTask<bool>(ref ConnectTrigger!);
    }

    private void Client_Connected(object? sender, EventArgs e) => ConnectTrigger?.Invoke(true);

    /// <summary>
    /// 输出控制台消息
    /// </summary>
    /// <param name="message">消息文本</param>
    /// <param name="isLine">是否是一整行</param>
    /// <returns></returns>
    public async Task OutputMessage(string message, bool isLine)
    {
        if (Client.IsConnected)
            await Client.SendTextMessage(new XFEDictionary("IsLine", isLine ? "true" : "false", "Text", message));
    }
}
