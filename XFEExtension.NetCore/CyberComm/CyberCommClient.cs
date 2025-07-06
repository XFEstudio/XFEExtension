using System.Net.WebSockets;
using System.Text;

namespace XFEExtension.NetCore.CyberComm;

/// <summary>
/// CyberComm客户端
/// </summary>
public class CyberCommClient
{
    private int reconnectTimes = -1;
    #region 公共属性
    /// <summary>
    /// 指定的WS服务器URL
    /// </summary>
    public string? ServerURL { get; set; }
    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected { get; private set; } = false;
    /// <summary>
    /// 是否自动重连
    /// </summary>
    public bool AutoReconnect { get; set; }
    /// <summary>
    /// 自动重连最大次数
    /// </summary>
    public int ReconnectMaxTimes { get; set; } = -1;
    /// <summary>
    /// 自动重连尝试间隔
    /// </summary>
    public int ReconnectTryDelay { get; set; } = 100;
    /// <summary>
    /// 是否自动接收完整消息
    /// </summary>
    public bool AutoReceiveCompletedMessage { get; set; }
    /// <summary>
    /// 收到消息时触发
    /// </summary>
    public event EventHandler<CyberCommClientEventArgs>? MessageReceived;
    /// <summary>
    /// 连接关闭时触发
    /// </summary>
    public event EventHandler? ConnectionClosed;
    /// <summary>
    /// 连接成功时触发
    /// </summary>
    public event EventHandler? Connected;
    /// <summary>
    /// WebSocket客户端
    /// </summary>
    public ClientWebSocket ClientWebSocket { get; set; } = new ClientWebSocket();
    #endregion
    #region 公有方法
    /// <summary>
    /// 启动CyberComm客户端
    /// </summary>
    /// <returns></returns>
    public async Task StartCyberCommClient()
    {
    StartConnect:
        reconnectTimes++;
        try
        {
            Uri serverUri = new(ServerURL!);
            await ClientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
        }
        catch (Exception ex)
        {
            if (IsConnected == true)
            {
                ConnectionClosed?.Invoke(this, EventArgs.Empty);
            }
            IsConnected = false;
            if (AutoReconnect)
            {
                if (reconnectTimes <= ReconnectMaxTimes || ReconnectMaxTimes == -1)
                {
                    Thread.Sleep(ReconnectTryDelay);
                    goto StartConnect;
                }
            }
            else
            {
                MessageReceived?.Invoke(this, new CyberCommClientEventArgsImpl(ClientWebSocket, new XFECyberCommException("连接服务器时发生异常", ex)));
                return;
            }
        }
        IsConnected = true;
        Connected?.Invoke(this, EventArgs.Empty);
        reconnectTimes = 0;
        while (ClientWebSocket.State == WebSocketState.Open)
        {
            try
            {
                byte[] receiveBuffer = new byte[1024];
                WebSocketReceiveResult receiveResult = await ClientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                var bufferList = new List<byte>();
                bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                //ReceiveCompletedMessageByUsingWhile
                if (AutoReceiveCompletedMessage)
                {
                    while (!receiveResult.EndOfMessage)
                    {
                        receiveResult = await ClientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                        bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                    }
                }
                var receivedBinaryBuffer = bufferList.ToArray();
                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    var receivedMessage = Encoding.UTF8.GetString(receivedBinaryBuffer);
                    MessageReceived?.Invoke(this, new CyberCommClientEventArgsImpl(ClientWebSocket, receivedMessage, receiveResult.EndOfMessage));
                }
                if (receiveResult.MessageType == WebSocketMessageType.Binary)
                {
                    MessageReceived?.Invoke(this, new CyberCommClientEventArgsImpl(ClientWebSocket, receivedBinaryBuffer, receiveResult.EndOfMessage));
                }
                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    break;
                }
            }
            catch (Exception ex)
            {
                if (IsConnected == true)
                {
                    ConnectionClosed?.Invoke(this, EventArgs.Empty);
                }
                IsConnected = false;
                if (AutoReconnect)
                {
                    if (AutoReconnect)
                    {
                        Thread.Sleep(ReconnectTryDelay);
                        if (reconnectTimes <= ReconnectMaxTimes || ReconnectMaxTimes == -1)
                            goto StartConnect;
                    }
                }
                else
                {
                    MessageReceived?.Invoke(this, new CyberCommClientEventArgsImpl(ClientWebSocket, new XFECyberCommException("与服务器端通讯期间发生异常", ex)));
                    return;
                }
            }
        }
        if (AutoReconnect)
        {
            goto StartConnect;
        }
        ConnectionClosed?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// 发送文本消息
    /// </summary>
    /// <param name="message">待发送的文本</param>
    /// <returns>发送进程</returns>
    public async Task SendTextMessage(string message)
    {
        try
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
            await ClientWebSocket!.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("客户端发送文本到服务器时出现异常", ex);
        }
    }
    /// <summary>
    /// 发送二进制消息
    /// </summary>
    /// <param name="message">待发送的二进制数据</param>
    /// <returns>发送进程</returns>
    public async Task SendBinaryMessage(byte[] message)
    {
        try
        {
            await ClientWebSocket!.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Binary, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("客户端发送二进制数据到服务器时出现异常", ex);
        }
    }
    /// <summary>
    /// 关闭CyberComm客户端
    /// </summary>
    /// <returns></returns>
    public async Task CloseCyberCommClient()
    {
        await ClientWebSocket!.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    }
    #endregion
    #region 构造函数
    /// <summary>
    /// CyberComm客户端
    /// </summary>
    /// <param name="serverURL">WS服务器地址</param>
    /// <param name="autoReconnect">是否自动重连</param>
    /// <param name="autoReceiveCompletedMessage">是否自动接收完整消息</param>
    public CyberCommClient(string serverURL, bool autoReconnect = true, bool autoReceiveCompletedMessage = true)
    {
        AutoReconnect = autoReconnect;
        AutoReceiveCompletedMessage = autoReceiveCompletedMessage;
        ServerURL = serverURL;
    }
    /// <summary>
    /// CyberComm客户端
    /// </summary>
    public CyberCommClient() { }
    #endregion
}