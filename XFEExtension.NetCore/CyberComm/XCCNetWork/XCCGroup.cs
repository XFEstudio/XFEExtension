using System.Net.WebSockets;
using System.Text;
using System.Collections.Concurrent;
using XFEExtension.NetCore.ArrayExtension;
using XFEExtension.NetCore.BufferExtension;
using XFEExtension.NetCore.Exceptions;

namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

// TODO: 完善超时功能
/// <summary>
/// XCC群组
/// </summary>
public abstract class XCCGroup
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pendingAcknowledgements = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim _textSendGate = new(1, 1);
    private readonly SemaphoreSlim _fileSendGate = new(1, 1);
    private readonly XCCNetWorkBase _workBase;
    private int _reconnectTimes = -1;
    private bool _readyToClose;
    private readonly Uri _serverUri;
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
    public bool TextMessageClientConnected { get; private set; }
    /// <summary>
    /// 文件传输服务器是否连接
    /// </summary>
    public bool FileTransportClientConnected { get; private set; }
    /// <summary>
    /// WebSocket明文传输客户端
    /// </summary>
    public ClientWebSocket? TextMessageClientWebSocket { get; private set; }
    /// <summary>
    /// WebSocket文件传输客户端
    /// </summary>
    public ClientWebSocket? FileTransportClientWebSocket { get; private set; }
    /// <summary>允许发送的最大文本消息字节数。</summary>
    public int MaxTextMessageBytes { get; set; } = 1024 * 1024;
    /// <summary>允许发送的最大二进制消息字节数。</summary>
    public long MaxBinaryMessageBytes { get; set; } = 64 * 1024 * 1024;
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
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public async Task StartTextMessageXCC(bool autoReconnect = true, int reconnectMaxTimes = -1, int reconnectTryDelay = 100)
    {
    XCCReconnect:
        TextMessageClientWebSocket = new ClientWebSocket();
        var serverUri = _serverUri;
        var base64GroupId = Convert.ToBase64String(Encoding.UTF8.GetBytes(GroupId));
        var base64SenderId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Sender));
        TextMessageClientWebSocket.Options.SetRequestHeader("Group", base64GroupId);
        TextMessageClientWebSocket.Options.SetRequestHeader("Sender", base64SenderId);
        TextMessageClientWebSocket.Options.SetRequestHeader("Type", "Text");
        TextMessageClientWebSocket.Options.SetRequestHeader("Signature", Signature);
        _reconnectTimes++;
        try
        {
            if (TextMessageClientWebSocket.State != WebSocketState.Open)
                await TextMessageClientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
        }
        catch (Exception ex)
        {
            if (_readyToClose)
            {
                return;
            }
            if (TextMessageClientConnected)
            {
                _workBase.ConnectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.TextMessageClient, TextMessageClientWebSocket, FileTransportClientWebSocket, false));
            }
            TextMessageClientConnected = false;
            if (autoReconnect)
            {
                if (_reconnectTimes <= reconnectMaxTimes || reconnectMaxTimes == -1)
                {
                    await Task.Delay(reconnectTryDelay);
                    goto XCCReconnect;
                }
            }
            else
            {
                _workBase.ExceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.TextMessageClient, null, null, new XFECyberCommException("与XCC网络通讯明文服务器建立连接时发生异常", ex)));
                return;
            }
        }
        _reconnectTimes = 0;
        TextMessageClientConnected = true;
        _workBase.Connected?.Invoke(this, new XCCConnectedEventArgsImpl(this, XCCClientType.TextMessageClient, TextMessageClientWebSocket, FileTransportClientWebSocket));
        while (TextMessageClientWebSocket.State == WebSocketState.Open)
        {
            try
            {
                var receiveBuffer = new byte[1024];
                var receiveResult = await TextMessageClientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                var bufferList = new List<byte>();
                bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                //ReceiveCompletedMessageByUsingWhile
                while (!receiveResult.EndOfMessage)
                {
                    receiveResult = await TextMessageClientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                }
                var receivedBinaryBuffer = bufferList.ToArray();
                switch (receiveResult.MessageType)
                {
                    case WebSocketMessageType.Text:
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
                            var messageType = signature switch
                            {
                                "[XCCTextMessage]" => XCCTextMessageType.Text,
                                "[XCCImage]" => XCCTextMessageType.Image,
                                "[XCCAudio]" => XCCTextMessageType.Audio,
                                "[XCCVideo]" => XCCTextMessageType.Video,
                                _ => XCCTextMessageType.Text
                            };
                            _workBase.TextMessageReceived?.Invoke(this, new XCCTextMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.TextMessageClient, messageId, messageType, message, senderName, sendTime, isHistory));
                        }
                        catch (Exception ex)
                        {
                            _workBase.ExceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.TextMessageClient, null, null, new XFECyberCommException("接收XCC服务器消息时发生异常", ex)));
                        }

                        break;
                    case WebSocketMessageType.Binary:
                        try
                        {
                            var xFEBuffer = XFEBuffer.ToXFEBuffer(receivedBinaryBuffer);
                            var signature = Encoding.UTF8.GetString(xFEBuffer["Type"]);
                            var messageId = Encoding.UTF8.GetString(xFEBuffer["ID"]);
                            if (signature == "callback")
                                CompleteAcknowledgement(messageId);
                        }
                        catch (Exception ex)
                        {
                            _workBase.ExceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.TextMessageClient, null, null, new XFECyberCommException("接收XCC服务器消息时发生异常", ex)));
                        }

                        break;
                    case WebSocketMessageType.Close:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(receivedBinaryBuffer), "未知的WebSocket消息类型");
                }
            }
            catch (Exception ex)
            {
                try { await TextMessageClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None); } catch { }
                if (TextMessageClientConnected)
                {
                    _workBase.ConnectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.TextMessageClient, TextMessageClientWebSocket, FileTransportClientWebSocket, false));
                }
                TextMessageClientConnected = false;
                if (autoReconnect)
                {
                    if (autoReconnect)
                    {
                        await Task.Delay(reconnectTryDelay);
                        if (_reconnectTimes <= reconnectMaxTimes || reconnectMaxTimes == -1)
                            goto XCCReconnect;
                    }
                }
                else
                {
                    _workBase.ExceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.TextMessageClient, null, null, new XFECyberCommException("与XCC网络通讯服务器建立连接时发生异常", ex)));
                    return;
                }
            }
        }
        _workBase.ConnectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.TextMessageClient, TextMessageClientWebSocket, FileTransportClientWebSocket, true));
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
        var serverUri = _serverUri;
        var base64GroupId = Convert.ToBase64String(Encoding.UTF8.GetBytes(GroupId));
        var base64SenderId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Sender));
        FileTransportClientWebSocket.Options.SetRequestHeader("Group", base64GroupId);
        FileTransportClientWebSocket.Options.SetRequestHeader("Sender", base64SenderId);
        FileTransportClientWebSocket.Options.SetRequestHeader("Type", "File");
        FileTransportClientWebSocket.Options.SetRequestHeader("Signature", Signature);
        _reconnectTimes++;
        try
        {
            if (FileTransportClientWebSocket.State != WebSocketState.Open)
                await FileTransportClientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
        }
        catch (Exception ex)
        {
            if (_readyToClose)
            {
                return;
            }
            if (FileTransportClientConnected)
            {
                _workBase.ConnectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.FileTransportClient, TextMessageClientWebSocket, FileTransportClientWebSocket, false));
            }
            FileTransportClientConnected = false;
            if (autoReconnect)
            {
                if (_reconnectTimes <= reconnectMaxTimes || reconnectMaxTimes == -1)
                {
                    await Task.Delay(reconnectTryDelay);
                    goto XCCReconnect;
                }
            }
            else
            {
                _workBase.ExceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.FileTransportClient, null, null, new XFECyberCommException("与XCC网络通讯明文服务器建立连接时发生异常", ex)));
                return;
            }
        }
        _reconnectTimes = 0;
        FileTransportClientConnected = true;
        _workBase.Connected?.Invoke(this, new XCCConnectedEventArgsImpl(this, XCCClientType.FileTransportClient, TextMessageClientWebSocket, FileTransportClientWebSocket));
        while (FileTransportClientWebSocket.State == WebSocketState.Open)
        {
            try
            {
                var receiveBuffer = new byte[1024];
                var receiveResult = await FileTransportClientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                var bufferList = new List<byte>();
                bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                //ReceiveCompletedMessageByUsingWhile
                while (!receiveResult.EndOfMessage)
                {
                    receiveResult = await FileTransportClientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                }
                var receivedBinaryBuffer = bufferList.ToArray();
                if (receiveResult.MessageType != WebSocketMessageType.Binary)
                    continue;
                try
                {
                    XCCBinaryMessageType messageType;
                    var xFEBuffer = XFEBuffer.ToXFEBuffer(receivedBinaryBuffer);
                    var sender = Encoding.UTF8.GetString(xFEBuffer["Sender"]);
                    var signature = Encoding.UTF8.GetString(xFEBuffer["Type"]);
                    if (signature == "callback")
                        return;
                    var messageId = Encoding.UTF8.GetString(xFEBuffer["ID"]);
                    var isHistory = Encoding.UTF8.GetString(xFEBuffer["IsHistory"]) == "True";
                    var sendTime = DateTime.Parse(Encoding.UTF8.GetString(xFEBuffer["SendTime"]));
                    var unPackedBuffer = xFEBuffer[sender];
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
                            CompleteAcknowledgement(messageId);
                            continue;
                        default:
                            messageType = XCCBinaryMessageType.Binary;
                            break;
                    }
                    _workBase.BinaryMessageReceived?.Invoke(this, new XCCBinaryMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.FileTransportClient, sender, messageId, unPackedBuffer, messageType, signature, sendTime, isHistory));
                }
                catch (Exception ex)
                {
                    _workBase.ExceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.FileTransportClient, null, null, new XFECyberCommException("接收XCC服务器消息时发生异常", ex)));
                }
            }
            catch (Exception ex)
            {
                try { await FileTransportClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None); } catch { }
                if (FileTransportClientConnected)
                {
                    _workBase.ConnectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.FileTransportClient, TextMessageClientWebSocket, FileTransportClientWebSocket, false));
                }
                FileTransportClientConnected = false;
                if (autoReconnect)
                {
                    if (autoReconnect)
                    {
                        await Task.Delay(reconnectTryDelay);
                        if (_reconnectTimes <= reconnectMaxTimes || reconnectMaxTimes == -1)
                            goto XCCReconnect;
                    }
                }
                else
                {
                    _workBase.ExceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.FileTransportClient, null, null, new XFECyberCommException("与XCC网络通讯服务器建立连接时发生异常", ex)));
                    return;
                }
            }
        }
        _workBase.ConnectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.FileTransportClient, TextMessageClientWebSocket, FileTransportClientWebSocket, true));
    }
    /// <summary>
    /// 等待明文服务器和文件服务器均连接
    /// </summary>
    /// <returns></returns>
    public async Task WaitConnect(CancellationToken cancellationToken = default)
    {
        while (!TextMessageClientConnected || !FileTransportClientConnected)
            await Task.Delay(25, cancellationToken);
    }
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
            if (Encoding.UTF8.GetByteCount(message) > MaxTextMessageBytes)
                throw new XFECyberCommException("XCC 文本消息超过允许的大小");
            var sendBuffer = Encoding.UTF8.GetBytes(new[] { messageId, "[XCCTextMessage]", message }.ToXFEString());
            return await SendAndWaitForAcknowledgementAsync(messageId, timeout, _textSendGate, cancellationToken =>
                TextMessageClientWebSocket!.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, cancellationToken));
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
    // ReSharper disable once UnusedParameter.Global
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
            if (message.LongLength > MaxBinaryMessageBytes)
                throw new XFECyberCommException("XCC 二进制消息超过允许的大小");
            var xFEBuffer = new XFEBuffer(Sender, message, "Type", Encoding.UTF8.GetBytes(signature), "ID", Encoding.UTF8.GetBytes(messageId));
            return await SendAndWaitForAcknowledgementAsync(messageId, timeout, _fileSendGate, cancellationToken =>
                FileTransportClientWebSocket!.SendAsync(new ArraySegment<byte>(xFEBuffer.ToBuffer()), WebSocketMessageType.Binary, true, cancellationToken));
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
        EnsureFileSize(filePath);
        try { return await SendSignedBinaryMessage(await File.ReadAllBytesAsync(filePath), "image", 60000); } catch (Exception ex) { throw new XFECyberCommException("客户端发送图片到服务器时出现异常", ex); }
    }
    /// <summary>
    /// 发送视频
    /// </summary>
    /// <param name="filePath">视频路径</param>
    /// <returns>服务器接收校验是否成功</returns>
    /// <exception cref="XFECyberCommException"></exception>
    public async Task<bool> SendVideo(string filePath)
    {
        EnsureFileSize(filePath);
        try { return await SendSignedBinaryMessage(await File.ReadAllBytesAsync(filePath), "video", 300000); } catch (Exception ex) { throw new XFECyberCommException("客户端发送视频到服务器时出现异常", ex); }
    }
    /// <summary>
    /// 发送音频
    /// </summary>
    /// <param name="filePath">音频路径</param>
    /// <returns>服务器接收校验是否成功</returns>
    /// <exception cref="XFECyberCommException"></exception>
    public async Task<bool> SendAudio(string filePath)
    {
        EnsureFileSize(filePath);
        try { return await SendSignedBinaryMessage(await File.ReadAllBytesAsync(filePath), "audio"); } catch (Exception ex) { throw new XFECyberCommException("客户端发送音频到服务器时出现异常", ex); }
    }
    /// <summary>
    /// 发送音频字节流（服务器不会缓存）
    /// </summary>
    /// <param name="buffer">二进制音频流</param>
    /// <returns>服务器接收校验是否成功</returns>
    /// <exception cref="XFECyberCommException"></exception>
    public async Task<bool> SendAudioBuffer(byte[] buffer)
    {
        try { return await SendSignedBinaryMessage(buffer, "audio-buffer"); } catch (Exception ex) { throw new XFECyberCommException("客户端发送音频到服务器时出现异常", ex); }
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
            var sendBuffer = Encoding.UTF8.GetBytes(new[] { messageId, "[XCCGetHistory]", "[XCCGetHistory]" }.ToXFEString());
            return await SendAndWaitForAcknowledgementAsync(messageId, 5000, _textSendGate, cancellationToken =>
                TextMessageClientWebSocket!.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, cancellationToken));
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
            _readyToClose = true;
            await TextMessageClientWebSocket!.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端主动关闭连接", CancellationToken.None);
            await FileTransportClientWebSocket!.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端主动关闭连接", CancellationToken.None);
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("客户端关闭连接时出现异常", ex);
        }
    }

    private void CompleteAcknowledgement(string messageId)
    {
        if (_pendingAcknowledgements.TryGetValue(messageId, out var completion))
            completion.TrySetResult(true);
    }

    private void EnsureFileSize(string filePath)
    {
        var file = new FileInfo(filePath);
        if (!file.Exists) throw new FileNotFoundException("未找到待发送文件", filePath);
        if (file.Length > MaxBinaryMessageBytes) throw new XFECyberCommException("XCC 文件超过允许的大小");
    }

    private async Task<bool> SendAndWaitForAcknowledgementAsync(
        string messageId,
        int timeoutMilliseconds,
        SemaphoreSlim sendGate,
        Func<CancellationToken, Task> send)
    {
        if (timeoutMilliseconds <= 0) throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds));
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_pendingAcknowledgements.TryAdd(messageId, completion))
            throw new XFECyberCommException($"消息 ID '{messageId}' 正在等待 ACK，不能重复发送");
        using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMilliseconds));
        try
        {
            await sendGate.WaitAsync(timeout.Token).ConfigureAwait(false);
            try { await send(timeout.Token).ConfigureAwait(false); }
            finally { sendGate.Release(); }
            return await completion.Task.WaitAsync(timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            return false;
        }
        finally
        {
            _pendingAcknowledgements.TryRemove(new KeyValuePair<string, TaskCompletionSource<bool>>(messageId, completion));
        }
    }
    #endregion
    internal XCCGroup(string signature, string groupId, string sender, Uri serverUri, XCCNetWorkBase xCCNetWorkBase)
    {
        Signature = signature;
        GroupId = groupId;
        Sender = sender;
        _serverUri = serverUri;
        _workBase = xCCNetWorkBase;
    }
}
