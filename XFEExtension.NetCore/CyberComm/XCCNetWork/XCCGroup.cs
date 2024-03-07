using System.Net.WebSockets;
using System.Text;
using XFEExtension.NetCore.ArrayExtension;
using XFEExtension.NetCore.BufferExtension;
using XFEExtension.NetCore.TaskExtension;

namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// XCC群组
/// </summary>
public abstract class XCCGroup
{
    private event EndTaskTrigger<bool>? UpdateTaskTrigger;
    private readonly XCCNetWorkBase workBase;
    private int reconnectTimes = -1;
    private bool readyToClose = false;
    #region 公有属性
    /// <summary>
    /// 客户端标识名
    /// </summary>
    public string Signature { get; }
    /// <summary>
    /// 群组ID
    /// </summary>
    public string GroupId { get; }
    /// <summary>
    /// 发送者
    /// </summary>
    public string Sender { get; }
    /// <summary>
    /// 明文传输服务器是否连接
    /// </summary>
    public bool TextMessageClientConnected { get; private set; } = false;
    /// <summary>
    /// 文件传输服务器是否连接
    /// </summary>
    public bool FileTransportClientConnected { get; private set; } = false;
    /// <summary>
    /// WebSocket明文传输客户端
    /// </summary>
    public ClientWebSocket? TextMessageClientWebSocket { get; private set; }
    /// <summary>
    /// WebSocket文件传输客户端
    /// </summary>
    public ClientWebSocket? FileTransportClientWebSocket { get; private set; }
    #endregion
    #region 公有方法
    /// <summary>
    /// 启动XCC会话
    /// </summary>
    /// <param name="autoReconnect">是否自动重连</param>
    /// <param name="reconnectMaxTimes">最大重连次数，-1则为无限次</param>
    /// <param name="reconnectTryDelay">重连尝试延迟</param>
    /// <returns></returns>
    public async Task StartXCC(bool autoReconnect = true, int reconnectMaxTimes = -1, int reconnectTryDelay = 100)
    {
        var textMessageXCCTask = StartTextMessageXCC(autoReconnect, reconnectMaxTimes, reconnectTryDelay);
        var fileTransportXCCTask = StartFileTransportXCC(autoReconnect, reconnectMaxTimes, reconnectTryDelay);
        await Task.WhenAll(textMessageXCCTask, fileTransportXCCTask);
    }
    /// <summary>
    /// 启动XCC文本会话
    /// </summary>
    /// <param name="autoReconnect">是否自动重连</param>
    /// <param name="reconnectMaxTimes">最大重连次数，-1则为无限次</param>
    /// <param name="reconnectTryDelay">重连尝试延迟</param>
    /// <returns></returns>
    public async Task StartTextMessageXCC(bool autoReconnect = true, int reconnectMaxTimes = -1, int reconnectTryDelay = 100)
    {
    XCCReconnect:
        TextMessageClientWebSocket = new ClientWebSocket();
        var serverUri = new Uri("ws://xcc.api.xfegzs.com");
        var base64GroupId = Convert.ToBase64String(Encoding.UTF8.GetBytes(GroupId));
        var base64SenderId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Sender));
        TextMessageClientWebSocket.Options.SetRequestHeader("Group", base64GroupId);
        TextMessageClientWebSocket.Options.SetRequestHeader("Sender", base64SenderId);
        TextMessageClientWebSocket.Options.SetRequestHeader("Type", "Text");
        TextMessageClientWebSocket.Options.SetRequestHeader("Signature", Signature);
        reconnectTimes++;
        try
        {
            if (TextMessageClientWebSocket.State != WebSocketState.Open)
                await TextMessageClientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
        }
        catch (Exception ex)
        {
            if (readyToClose)
            {
                return;
            }
            if (TextMessageClientConnected == true)
            {
                workBase.connectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.TextMessageClient, TextMessageClientWebSocket, FileTransportClientWebSocket, false));
            }
            TextMessageClientConnected = false;
            if (autoReconnect)
            {
                if (reconnectTimes <= reconnectMaxTimes || reconnectMaxTimes == -1)
                {
                    Thread.Sleep(reconnectTryDelay);
                    goto XCCReconnect;
                }
            }
            else
            {
                workBase.exceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.TextMessageClient, null, null, new XFECyberCommException("与XCC网络通讯明文服务器建立连接时发生异常", ex)));
                return;
            }
        }
        reconnectTimes = 0;
        TextMessageClientConnected = true;
        workBase.connected?.Invoke(this, new XCCConnectedEventArgsImpl(this, XCCClientType.TextMessageClient, TextMessageClientWebSocket, FileTransportClientWebSocket));
        while (TextMessageClientWebSocket.State == WebSocketState.Open)
        {
            try
            {
                byte[] receiveBuffer = new byte[1024];
                WebSocketReceiveResult receiveResult = await TextMessageClientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                var bufferList = new List<byte>();
                bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                //ReceiveCompletedMessageByUsingWhile
                while (!receiveResult.EndOfMessage)
                {
                    receiveResult = await TextMessageClientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                }
                var receivedBinaryBuffer = bufferList.ToArray();
                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    try
                    {
                        var receivedMessage = Encoding.UTF8.GetString(receivedBinaryBuffer);
                        var isHistory = receivedMessage.StartsWith("[XCCGetHistory]");
                        if (isHistory)
                        {
                            receivedMessage = receivedMessage[15..];
                        }
                        var unPackedMessage = receivedMessage.ToXFEArray<string>();
                        var messageId = unPackedMessage[0];
                        var signature = unPackedMessage[1];
                        var message = unPackedMessage[2];
                        var senderName = unPackedMessage[3];
                        var sendTime = DateTime.Parse(unPackedMessage[4]);
                        var messageType = XCCTextMessageType.Text;
                        switch (signature)
                        {
                            case "[XCCTextMessage]":
                                messageType = XCCTextMessageType.Text;
                                break;
                            case "[XCCImage]":
                                messageType = XCCTextMessageType.Image;
                                break;
                            case "[XCCAudio]":
                                messageType = XCCTextMessageType.Audio;
                                break;
                            case "[XCCVideo]":
                                messageType = XCCTextMessageType.Video;
                                break;
                            default:
                                break;
                        }
                        workBase.textMessageReceived?.Invoke(this, new XCCTextMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.TextMessageClient, messageId, messageType, message, senderName, sendTime, isHistory));
                    }
                    catch (Exception ex)
                    {
                        workBase.exceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.TextMessageClient, null, null, new XFECyberCommException("接收XCC服务器消息时发生异常", ex)));
                    }
                }
                else if (receiveResult.MessageType == WebSocketMessageType.Binary)
                {
                    try
                    {
                        var xFEBuffer = XFEBuffer.ToXFEBuffer(receivedBinaryBuffer);
                        var signature = Encoding.UTF8.GetString(xFEBuffer["Type"]);
                        var messageId = Encoding.UTF8.GetString(xFEBuffer["ID"]);
                        if (signature == "callback")
                            UpdateTaskTrigger?.Invoke(true, messageId);
                    }
                    catch (Exception ex)
                    {
                        workBase.exceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.TextMessageClient, null, null, new XFECyberCommException("接收XCC服务器消息时发生异常", ex)));
                    }
                }
            }
            catch (Exception ex)
            {
                try { await TextMessageClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None); } catch { }
                if (TextMessageClientConnected == true)
                {
                    workBase.connectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.TextMessageClient, TextMessageClientWebSocket, FileTransportClientWebSocket, false));
                }
                TextMessageClientConnected = false;
                if (autoReconnect)
                {
                    if (autoReconnect)
                    {
                        Thread.Sleep(reconnectTryDelay);
                        if (reconnectTimes <= reconnectMaxTimes || reconnectMaxTimes == -1)
                            goto XCCReconnect;
                    }
                }
                else
                {
                    workBase.exceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.TextMessageClient, null, null, new XFECyberCommException("与XCC网络通讯服务器建立连接时发生异常", ex)));
                    return;
                }
            }
        }
        workBase.connectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.TextMessageClient, TextMessageClientWebSocket, FileTransportClientWebSocket, true));
    }
    /// <summary>
    /// 启动XCC文件传输会话
    /// </summary>
    /// <param name="autoReconnect">是否自动重连</param>
    /// <param name="reconnectMaxTimes">最大重连次数，-1则为无限次</param>
    /// <param name="reconnectTryDelay">重连尝试延迟</param>
    /// <returns></returns>
    public async Task StartFileTransportXCC(bool autoReconnect = true, int reconnectMaxTimes = -1, int reconnectTryDelay = 100)
    {
    XCCReconnect:
        FileTransportClientWebSocket = new ClientWebSocket();
        Uri serverUri = new("ws://xcc.api.xfegzs.com");
        var base64GroupId = Convert.ToBase64String(Encoding.UTF8.GetBytes(GroupId));
        var base64SenderId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Sender));
        FileTransportClientWebSocket.Options.SetRequestHeader("Group", base64GroupId);
        FileTransportClientWebSocket.Options.SetRequestHeader("Sender", base64SenderId);
        FileTransportClientWebSocket.Options.SetRequestHeader("Type", "File");
        FileTransportClientWebSocket.Options.SetRequestHeader("Signature", Signature);
        reconnectTimes++;
        try
        {
            if (FileTransportClientWebSocket.State != WebSocketState.Open)
                await FileTransportClientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
        }
        catch (Exception ex)
        {
            if (readyToClose)
            {
                return;
            }
            if (FileTransportClientConnected == true)
            {
                workBase.connectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.FileTransportClient, TextMessageClientWebSocket, FileTransportClientWebSocket, false));
            }
            FileTransportClientConnected = false;
            if (autoReconnect)
            {
                if (reconnectTimes <= reconnectMaxTimes || reconnectMaxTimes == -1)
                {
                    Thread.Sleep(reconnectTryDelay);
                    goto XCCReconnect;
                }
            }
            else
            {
                workBase.exceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.FileTransportClient, null, null, new XFECyberCommException("与XCC网络通讯明文服务器建立连接时发生异常", ex)));
                return;
            }
        }
        reconnectTimes = 0;
        FileTransportClientConnected = true;
        workBase.connected?.Invoke(this, new XCCConnectedEventArgsImpl(this, XCCClientType.FileTransportClient, TextMessageClientWebSocket, FileTransportClientWebSocket));
        while (FileTransportClientWebSocket.State == WebSocketState.Open)
        {
            try
            {
                byte[] receiveBuffer = new byte[1024];
                WebSocketReceiveResult receiveResult = await FileTransportClientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                var bufferList = new List<byte>();
                bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                //ReceiveCompletedMessageByUsingWhile
                while (!receiveResult.EndOfMessage)
                {
                    receiveResult = await FileTransportClientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                }
                var receivedBinaryBuffer = bufferList.ToArray();
                if (receiveResult.MessageType == WebSocketMessageType.Binary)
                {
                    try
                    {
                        var messageType = XCCBinaryMessageType.Binary;
                        var xFEBuffer = XFEBuffer.ToXFEBuffer(receivedBinaryBuffer);
                        var sender = Encoding.UTF8.GetString(xFEBuffer["Sender"]);
                        var signature = Encoding.UTF8.GetString(xFEBuffer["Type"]);
                        if (signature == "callback")
                            return;
                        var messageId = Encoding.UTF8.GetString(xFEBuffer["ID"]);
                        bool isHistory = Encoding.UTF8.GetString(xFEBuffer["IsHistory"]) == "True";
                        var sendTime = DateTime.Parse(Encoding.UTF8.GetString(xFEBuffer["SendTime"]));
                        byte[] unPackedBuffer = xFEBuffer[sender];
                        switch (signature)
                        {
                            case "text":
                                messageType = XCCBinaryMessageType.Text;
                                break;
                            case "image":
                                messageType = XCCBinaryMessageType.Image;
                                break;
                            case "audio":
                                messageType = XCCBinaryMessageType.Audio;
                                break;
                            case "audio-buffer":
                                messageType = XCCBinaryMessageType.AudioBuffer;
                                break;
                            case "video":
                                messageType = XCCBinaryMessageType.Video;
                                break;
                            case "callback":
                                UpdateTaskTrigger?.Invoke(true, messageId);
                                continue;
                            default:
                                messageType = XCCBinaryMessageType.Binary;
                                break;
                        }
                        workBase.binaryMessageReceived?.Invoke(this, new XCCBinaryMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.FileTransportClient, sender, messageId, unPackedBuffer, messageType, signature, sendTime, isHistory));
                    }
                    catch (Exception ex)
                    {
                        workBase.exceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.FileTransportClient, null, null, new XFECyberCommException("接收XCC服务器消息时发生异常", ex)));
                    }
                }
            }
            catch (Exception ex)
            {
                try { await FileTransportClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None); } catch { }
                if (FileTransportClientConnected == true)
                {
                    workBase.connectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.FileTransportClient, TextMessageClientWebSocket, FileTransportClientWebSocket, false));
                }
                FileTransportClientConnected = false;
                if (autoReconnect)
                {
                    if (autoReconnect)
                    {
                        Thread.Sleep(reconnectTryDelay);
                        if (reconnectTimes <= reconnectMaxTimes || reconnectMaxTimes == -1)
                            goto XCCReconnect;
                    }
                }
                else
                {
                    workBase.exceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.FileTransportClient, null, null, new XFECyberCommException("与XCC网络通讯服务器建立连接时发生异常", ex)));
                    return;
                }
            }
        }
        workBase.connectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.FileTransportClient, TextMessageClientWebSocket, FileTransportClientWebSocket, true));
    }
    /// <summary>
    /// 等待明文服务器和文件服务器均连接
    /// </summary>
    /// <returns></returns>
    public async Task WaitConnect() => await Task.Run(() => { while (!TextMessageClientConnected || !FileTransportClientConnected) { } });
    /// <summary>
    /// 发送文本消息
    /// </summary>
    /// <param name="message">待发送的文本</param>
    /// <param name="timeout">最长超时时长</param>
    /// <returns>服务器接收校验是否成功</returns>
    public async Task<bool> SendTextMessage(string message, int timeout = 30000) => await SendTextMessage(message, Guid.NewGuid().ToString(), timeout);
    /// <summary>
    /// 发送文本消息
    /// </summary>
    /// <param name="message">待发送的文本</param>
    /// <param name="messageId">消息ID</param>
    /// <param name="timeout">最长超时时长</param>
    /// <exception cref="XFECyberCommException"></exception>
    /// <returns>服务器接收校验是否成功</returns>
    public async Task<bool> SendTextMessage(string message, string messageId, int timeout)
    {
        try
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(new string[] { messageId, "[XCCTextMessage]", message }.ToXFEString());
            await TextMessageClientWebSocket!.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
            var endTask = Task.Run(async () =>
            {
                await Task.Delay(timeout);
                UpdateTaskTrigger?.Invoke(false, messageId);
            });
            return await new XFEWaitTask<bool>(ref UpdateTaskTrigger!, messageId);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("客户端发送文本到服务器时出现异常", ex);
        }
    }
    /// <summary>
    /// 发送标准的文本消息
    /// </summary>
    /// <param name="role">发送者角色</param>
    /// <param name="message">待发送的文本</param>
    /// <exception cref="XFECyberCommException"></exception>
    /// <returns>服务器接收校验是否成功</returns>
    [Obsolete("发送者已统一，请使用SendTextMessage或SendBinaryTextMessage")]
    public async Task<bool> SendStandardTextMessage(string role, string message)
    {
        try { return await SendTextMessage(message); } catch (Exception ex) { throw new XFECyberCommException("客户端发送文本到服务器时出现异常", ex); }
    }
    /// <summary>
    /// 发送签名二进制消息
    /// </summary>
    /// <param name="message">二进制消息</param>
    /// <param name="signature">签名标识</param>
    /// <param name="timeout">最长超时时长</param>
    /// <returns>服务器接收校验是否成功</returns>
    public async Task<bool> SendSignedBinaryMessage(byte[] message, string signature, int timeout = 10000) => await SendSignedBinaryMessage(message, Guid.NewGuid().ToString(), signature, timeout);
    /// <summary>
    /// 发送签名二进制消息
    /// </summary>
    /// <param name="message">二进制消息</param>
    /// <param name="messageId">消息ID</param>
    /// <param name="signature">签名标识</param>
    /// <param name="timeout">最长超时时长</param>
    /// <returns>服务器接收校验是否成功</returns>
    /// <exception cref="XFECyberCommException"></exception>
    public async Task<bool> SendSignedBinaryMessage(byte[] message, string messageId, string signature, int timeout)
    {
        try
        {
            var xFEBuffer = new XFEBuffer(Sender, message, "Type", Encoding.UTF8.GetBytes(signature), "ID", Encoding.UTF8.GetBytes(messageId));
            await FileTransportClientWebSocket!.SendAsync(new ArraySegment<byte>(xFEBuffer.ToBuffer()), WebSocketMessageType.Binary, true, CancellationToken.None);
            var endTask = Task.Run(async () =>
            {
                await Task.Delay(timeout);
                UpdateTaskTrigger?.Invoke(false, messageId);
            });
            return await new XFEWaitTask<bool>(ref UpdateTaskTrigger!, messageId);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("客户端发送二进制数据到服务器时出现异常", ex);
        }
    }
    /// <summary>
    /// 发送二进制文本消息
    /// </summary>
    /// <param name="message">消息</param>
    /// <returns>服务器接收校验是否成功</returns>
    /// <exception cref="XFECyberCommException"></exception>
    public async Task<bool> SendBinaryTextMessage(string message)
    {
        try { return await SendSignedBinaryMessage(Encoding.UTF8.GetBytes(message), "text"); } catch (Exception ex) { throw new XFECyberCommException("客户端发送文本到服务器时出现异常", ex); }
    }
    /// <summary>
    /// 发送默认标准的二进制消息
    /// </summary>
    /// <param name="message">待发送的二进制数据</param>
    /// <param name="timeout">最长超时时长</param>
    /// <exception cref="XFECyberCommException"></exception>
    /// <returns>服务器接收校验是否成功</returns>
    public async Task<bool> SendBinaryMessage(byte[] message, int timeout = 1000)
    {
        try { return await SendSignedBinaryMessage(message, "binary", timeout); } catch (Exception ex) { throw new XFECyberCommException("客户端发送二进制数据到服务器时出现异常", ex); }
    }
    /// <summary>
    /// 发送图片
    /// </summary>
    /// <param name="filePath">图片路径</param>
    /// <returns>服务器接收校验是否成功</returns>
    /// <exception cref="XFECyberCommException"></exception>
    public async Task<bool> SendImage(string filePath)
    {
        try { return await SendSignedBinaryMessage(File.ReadAllBytes(filePath), "image", 60000); } catch (Exception ex) { throw new XFECyberCommException("客户端发送图片到服务器时出现异常", ex); }
    }
    /// <summary>
    /// 发送视频
    /// </summary>
    /// <param name="filePath">视频路径</param>
    /// <returns>服务器接收校验是否成功</returns>
    /// <exception cref="XFECyberCommException"></exception>
    public async Task<bool> SendVideo(string filePath)
    {
        try { return await SendSignedBinaryMessage(File.ReadAllBytes(filePath), "video", 300000); } catch (Exception ex) { throw new XFECyberCommException("客户端发送视频到服务器时出现异常", ex); }
    }
    /// <summary>
    /// 发送音频
    /// </summary>
    /// <param name="filePath">音频路径</param>
    /// <returns>服务器接收校验是否成功</returns>
    /// <exception cref="XFECyberCommException"></exception>
    public async Task<bool> SendAudio(string filePath)
    {
        try { return await SendSignedBinaryMessage(File.ReadAllBytes(filePath), "audio"); } catch (Exception ex) { throw new XFECyberCommException("客户端发送音频到服务器时出现异常", ex); }
    }
    /// <summary>
    /// 发送音频字节流（服务器不会缓存）
    /// </summary>
    /// <param name="buffer">二进制音频流</param>
    /// <returns>服务器接收校验是否成功</returns>
    /// <exception cref="XFECyberCommException"></exception>
    public async Task<bool> SendAudioBuffer(byte[] buffer)
    {
        try
        {
            return await SendSignedBinaryMessage(buffer, "audio-buffer");
        }
        catch (Exception ex) { throw new XFECyberCommException("客户端发送音频到服务器时出现异常", ex); }
    }
    /// <summary>
    /// 获取历史记录
    /// </summary>
    /// <exception cref="XFECyberCommException"></exception>
    public async Task<bool> GetHistory()
    {
        try
        {
            var messageId = Guid.NewGuid().ToString();
            byte[] sendBuffer = Encoding.UTF8.GetBytes(new string[] { messageId, "[XCCGetHistory]", "[XCCGetHistory]" }.ToXFEString());
            await TextMessageClientWebSocket!.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
            var endTask = Task.Run(async () =>
            {
                await Task.Delay(5000);
                UpdateTaskTrigger?.Invoke(false, messageId);
            });
            return await new XFEWaitTask<bool>(ref UpdateTaskTrigger!, messageId);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("客户端获取历史记录时出现异常", ex);
        }
    }
    /// <summary>
    /// 关闭XCC会话
    /// </summary>
    /// <returns></returns>
    /// <exception cref="XFECyberCommException"></exception>
    public async Task CloseXCC()
    {
        try
        {
            readyToClose = true;
            await TextMessageClientWebSocket!.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端主动关闭连接", CancellationToken.None);
            await FileTransportClientWebSocket!.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端主动关闭连接", CancellationToken.None);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("客户端关闭连接时出现异常", ex);
        }
    }
    #endregion
    internal XCCGroup(string signature, string groupId, string sender, XCCNetWorkBase xCCNetWorkBase)
    {
        Signature = signature;
        GroupId = groupId;
        Sender = sender;
        workBase = xCCNetWorkBase;
    }
}
