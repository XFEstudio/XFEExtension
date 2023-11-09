using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XFE各类拓展.ArrayExtension;
using XFE各类拓展.BufferExtension;
using XFE各类拓展.DelegateExtension;
using XFE各类拓展.FormatExtension;
using XFE各类拓展.TaskExtension;

namespace XFE各类拓展.CyberComm.XCCNetWork
{
    class XCCNetWorkBase
    {
        /// <summary>
        /// 明文消息接收时触发
        /// </summary>
        public EventHandler<XCCTextMessageReceivedEventArgs> textMessageReceived;
        /// <summary>
        /// 二进制消息接收时触发
        /// </summary>
        public EventHandler<XCCBinaryMessageReceivedEventArgs> binaryMessageReceived;
        /// <summary>
        /// 异常消息接收时触发
        /// </summary>
        public EventHandler<XCCExceptionMessageReceivedEventArgs> exceptionMessageReceived;
        /// <summary>
        /// 连接关闭时触发
        /// </summary>
        public EventHandler<XCCConnectionClosedEventArgs> connectionClosed;
        /// <summary>
        /// 连接成功时触发
        /// </summary>
        public EventHandler<XCCConnectedEventArgs> connected;
    }
    /// <summary>
    /// XCC客户端连接类型
    /// </summary>
    public enum XCCClientType
    {
        /// <summary>
        /// 明文类型
        /// </summary>
        TextMessageClient,
        /// <summary>
        /// 文件类型
        /// </summary>
        FileTransportClient
    }
    /// <summary>
    /// XCC网络通讯
    /// </summary>
    public class XCCNetWork
    {
        private readonly XCCNetWorkBase xCCNetWorkBase;
        /// <summary>
        /// XCC当前群组
        /// </summary>
        public List<XCCGroup> Groups { get; set; }
        /// <summary>
        /// 明文消息接收时触发
        /// </summary>
        public event EventHandler<XCCTextMessageReceivedEventArgs> TextMessageReceived
        {
            add
            {
                xCCNetWorkBase.textMessageReceived += value;
            }
            remove
            {
                xCCNetWorkBase.textMessageReceived -= value;
            }
        }
        /// <summary>
        /// 二进制消息接收时触发
        /// </summary>
        public event EventHandler<XCCBinaryMessageReceivedEventArgs> BinaryMessageReceived
        {
            add
            {
                xCCNetWorkBase.binaryMessageReceived += value;
            }
            remove
            {
                xCCNetWorkBase.binaryMessageReceived -= value;
            }
        }
        /// <summary>
        /// 异常消息接收时触发
        /// </summary>
        public event EventHandler<XCCExceptionMessageReceivedEventArgs> ExceptionMessageReceived
        {
            add
            {
                xCCNetWorkBase.exceptionMessageReceived += value;
            }
            remove
            {
                xCCNetWorkBase.exceptionMessageReceived -= value;
            }
        }
        /// <summary>
        /// 连接关闭时触发
        /// </summary>
        public event EventHandler<XCCConnectionClosedEventArgs> ConnectionClosed
        {
            add
            {
                xCCNetWorkBase.connectionClosed += value;
            }
            remove
            {
                xCCNetWorkBase.connectionClosed -= value;
            }
        }
        /// <summary>
        /// 连接成功时触发
        /// </summary>
        public event EventHandler<XCCConnectedEventArgs> Connected
        {
            add
            {
                xCCNetWorkBase.connected += value;
            }
            remove
            {
                xCCNetWorkBase.connected -= value;
            }
        }
        /// <summary>
        /// 创建XCC群组会话
        /// </summary>
        /// <param name="groupId">群组名称</param>
        /// <param name="sender">发送者</param>
        /// <returns></returns>
        public XCCGroup CreateGroup(string groupId, string sender)
        {
            var group = new XCCGroupImpl(groupId, sender, xCCNetWorkBase);
            Groups.Add(group);
            return group;
        }
        /// <summary>
        /// XCC网络通信会话
        /// </summary>
        public XCCNetWork()
        {
            xCCNetWorkBase = new XCCNetWorkBase();
            Groups = new List<XCCGroup>();
        }
    }
    /// <summary>
    /// XCC群组
    /// </summary>
    public abstract class XCCGroup
    {
        private event EndTaskTrigger<bool> UpdateTaskTrigger;
        private readonly XCCNetWorkBase workBase;
        private int reconnectTimes = -1;
        #region 公有属性
        /// <summary>
        /// 群组ID
        /// </summary>
        public string GroupId { get; }
        /// <summary>
        /// 发送者
        /// </summary>
        public string Sender { get; }
        /// <summary>
        /// WebSocket明文传输客户端
        /// </summary>
        public ClientWebSocket TextMessageClientWebSocket { get; private set; }
        /// <summary>
        /// WebSocket文件传输客户端
        /// </summary>
        public ClientWebSocket FileTransportClientWebSocket { get; private set; }
        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected { get; private set; } = false;
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
            Uri serverUri = new Uri("ws://xcc.api.xfegzs.com");
            var base64GroupId = Convert.ToBase64String(Encoding.UTF8.GetBytes(GroupId));
            var base64SenderId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Sender));
            TextMessageClientWebSocket.Options.SetRequestHeader("Group", base64GroupId);
            TextMessageClientWebSocket.Options.SetRequestHeader("Sender", base64SenderId);
            TextMessageClientWebSocket.Options.SetRequestHeader("Type", "Text");
            reconnectTimes++;
            try
            {
                if (TextMessageClientWebSocket.State != WebSocketState.Open)
                    await TextMessageClientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
            }
            catch (Exception ex)
            {
                if (IsConnected == true)
                {
                    workBase.connectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.TextMessageClient, TextMessageClientWebSocket, FileTransportClientWebSocket, false));
                }
                IsConnected = false;
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
            workBase.connected?.Invoke(this, new XCCConnectedEventArgsImpl(this, XCCClientType.TextMessageClient, TextMessageClientWebSocket, FileTransportClientWebSocket));
            IsConnected = true;
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
                            var isHistory = receivedMessage.IndexOf("[XCCGetHistory]") == 0;
                            if (isHistory)
                            {
                                receivedMessage = receivedMessage.Substring(15);
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
                }
                catch (Exception ex)
                {
                    try { await TextMessageClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None); } catch { }
                    if (IsConnected == true)
                    {
                        workBase.connectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.TextMessageClient, TextMessageClientWebSocket, FileTransportClientWebSocket, false));
                    }
                    IsConnected = false;
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
            Uri serverUri = new Uri("ws://xcc.api.xfegzs.com");
            var base64GroupId = Convert.ToBase64String(Encoding.UTF8.GetBytes(GroupId));
            var base64SenderId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Sender));
            FileTransportClientWebSocket.Options.SetRequestHeader("Group", base64GroupId);
            FileTransportClientWebSocket.Options.SetRequestHeader("Sender", base64SenderId);
            FileTransportClientWebSocket.Options.SetRequestHeader("Type", "File");
            reconnectTimes++;
            try
            {
                if (FileTransportClientWebSocket.State != WebSocketState.Open)
                    await FileTransportClientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
            }
            catch (Exception ex)
            {
                if (IsConnected == true)
                {
                    workBase.connectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.FileTransportClient, TextMessageClientWebSocket, FileTransportClientWebSocket, false));
                }
                IsConnected = false;
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
            workBase.connected?.Invoke(this, new XCCConnectedEventArgsImpl(this, XCCClientType.FileTransportClient, TextMessageClientWebSocket, FileTransportClientWebSocket));
            IsConnected = true;
            while (FileTransportClientWebSocket.State == WebSocketState.Open)
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
                            var isHistory = receivedMessage.IndexOf("[XCCGetHistory]") == 0;
                            if (isHistory)
                            {
                                receivedMessage = receivedMessage.Substring(15);
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
                            workBase.textMessageReceived?.Invoke(this, new XCCTextMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.FileTransportClient, messageId, messageType, message, senderName, sendTime, isHistory));
                        }
                        catch (Exception ex)
                        {
                            workBase.exceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.FileTransportClient, null, null, new XFECyberCommException("接收XCC服务器消息时发生异常", ex)));
                        }
                    }
                    if (receiveResult.MessageType == WebSocketMessageType.Binary)
                    {
                        try
                        {
                            var messageType = XCCBinaryMessageType.Binary;
                            var xFEBuffer = XFEBuffer.ToXFEBuffer(receivedBinaryBuffer);
                            var sender = Encoding.UTF8.GetString(xFEBuffer["Sender"]);
                            var signature = Encoding.UTF8.GetString(xFEBuffer["Type"]);
                            var messageId = Encoding.UTF8.GetString(xFEBuffer["ID"]);
                            byte[] unPackedBuffer = signature == "callback" ? null : xFEBuffer[sender];
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
                            workBase.binaryMessageReceived?.Invoke(this, new XCCBinaryMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.FileTransportClient, sender, messageId, unPackedBuffer, messageType, signature));
                        }
                        catch (Exception ex)
                        {
                            workBase.exceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, TextMessageClientWebSocket, FileTransportClientWebSocket, XCCClientType.FileTransportClient, null, null, new XFECyberCommException("接收XCC服务器消息时发生异常", ex)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    try { await TextMessageClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None); } catch { }
                    if (IsConnected == true)
                    {
                        workBase.connectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, XCCClientType.FileTransportClient, TextMessageClientWebSocket, FileTransportClientWebSocket, false));
                    }
                    IsConnected = false;
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
        /// 发送文本消息，返回消息ID
        /// </summary>
        /// <param name="message">待发送的文本</param>
        /// <param name="timeout">最长超时时长</param>
        /// <returns>服务器接收校验是否成功</returns>
        public async Task<bool> SendTextMessage(string message, int timeout = 30000)
        {
            var messageId = Guid.NewGuid().ToString();
            return await SendTextMessage(message, messageId, timeout);
        }
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
                await TextMessageClientWebSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                var endTask = Task.Run(async () =>
                {
                    await Task.Delay(timeout);
                    UpdateTaskTrigger?.Invoke(false, messageId);
                });
                return await new XFEWaitTask<bool>(ref UpdateTaskTrigger, messageId);
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
            try
            {
                return await SendTextMessage(message);
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("客户端发送文本到服务器时出现异常", ex);
            }
        }
        /// <summary>
        /// 发送签名二进制消息，返回消息ID
        /// </summary>
        /// <param name="message">二进制消息</param>
        /// <param name="signature">签名标识</param>
        /// <param name="timeout">最长超时时长</param>
        /// <returns>服务器接收校验是否成功</returns>
        public async Task<bool> SendSignedBinaryMessage(byte[] message, string signature, int timeout = 10000)
        {
            var messageId = Guid.NewGuid().ToString();
            return await SendSignedBinaryMessage(message, messageId, signature, timeout);
        }
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
                await TextMessageClientWebSocket.SendAsync(new ArraySegment<byte>(xFEBuffer.ToBuffer()), WebSocketMessageType.Binary, true, CancellationToken.None);
                var endTask = Task.Run(async () =>
                {
                    await Task.Delay(timeout);
                    UpdateTaskTrigger?.Invoke(false, messageId);
                });
                return await new XFEWaitTask<bool>(ref UpdateTaskTrigger, messageId);
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
            try
            {
                return await SendSignedBinaryMessage(Encoding.UTF8.GetBytes(message), "text");
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("客户端发送文本到服务器时出现异常", ex);
            }
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
            try
            {
                return await SendSignedBinaryMessage(message, "binary", timeout);
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("客户端发送二进制数据到服务器时出现异常", ex);
            }
        }
        /// <summary>
        /// 发送图片
        /// </summary>
        /// <param name="filePath">图片路径</param>
        /// <returns>服务器接收校验是否成功</returns>
        /// <exception cref="XFECyberCommException"></exception>
        public async Task<bool> SendImage(string filePath)
        {
            try
            {
                return await SendSignedBinaryMessage(File.ReadAllBytes(filePath), "image", 60000);
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("客户端发送图片到服务器时出现异常", ex);
            }
        }
        /// <summary>
        /// 发送视频
        /// </summary>
        /// <param name="filePath">视频路径</param>
        /// <returns>服务器接收校验是否成功</returns>
        /// <exception cref="XFECyberCommException"></exception>
        public async Task<bool> SendVideo(string filePath)
        {
            try
            {
                return await SendSignedBinaryMessage(File.ReadAllBytes(filePath), "video", 300000);
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("客户端发送视频到服务器时出现异常", ex);
            }
        }
        /// <summary>
        /// 发送音频
        /// </summary>
        /// <param name="filePath">音频路径</param>
        /// <returns>服务器接收校验是否成功</returns>
        /// <exception cref="XFECyberCommException"></exception>
        public async Task<bool> SendAudio(string filePath)
        {
            try
            {
                return await SendSignedBinaryMessage(File.ReadAllBytes(filePath), "audio");
            }
            catch (Exception ex) { throw new XFECyberCommException("客户端发送音频到服务器时出现异常", ex); }
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
                await TextMessageClientWebSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                var endTask = Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    UpdateTaskTrigger?.Invoke(false, messageId);
                });
                return await new XFEWaitTask<bool>(ref UpdateTaskTrigger, messageId);
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
                await TextMessageClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端主动关闭连接", CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("客户端关闭连接时出现异常", ex);
            }
        }
        #endregion
        internal XCCGroup(string groupId, string sender, XCCNetWorkBase xCCNetWorkBase)
        {
            GroupId = groupId;
            Sender = sender;
            workBase = xCCNetWorkBase;
        }
    }
    /// <summary>
    /// XCC文件类型
    /// </summary>
    public enum XCCFileType
    {
        /// <summary>
        /// 图片
        /// </summary>
        Image,
        /// <summary>
        /// 视频
        /// </summary>
        Video,
        /// <summary>
        /// 音频
        /// </summary>
        Audio
    }
    /// <summary>
    /// 消息接收触发器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="isHistory">是否为历史消息</param>
    /// <param name="message">消息</param>
    public delegate void MessageReceivedHandler<T>(bool isHistory, T message);
    /// <summary>
    /// XCC消息接收器
    /// </summary>
    public class XCCMessageReceiveHelper
    {
        private readonly Dictionary<string, XCCFile> xCCFileDictionary = new Dictionary<string, XCCFile>();
        private readonly Dictionary<string, List<XCCMessage>> xCCMessageDictionary = new Dictionary<string, List<XCCMessage>>();
        private bool loaded = false;
        /// <summary>
        /// 自动保存到本地
        /// </summary>
        public bool AutoSaveInLocal { get; set; }
        /// <summary>
        /// 保存的根目录
        /// </summary>
        public string SavePathRoot { get; set; }
        /// <summary>
        /// 接收到文件事件
        /// </summary>
        public event MessageReceivedHandler<XCCFile> FileReceived;
        /// <summary>
        /// 接收到文本事件
        /// </summary>
        public event MessageReceivedHandler<XCCMessage> TextReceived;
        /// <summary>
        /// 从设置的根目录加载
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(SavePathRoot))
                {
                    foreach (var groupIdFullPath in Directory.EnumerateDirectories(SavePathRoot))
                    {
                        var groupId = Path.GetFileName(groupIdFullPath);
                        if (File.Exists($"{groupIdFullPath}/XFEMessage/XFEMessage.xfe"))
                        {
                            var xCCMessageList = new List<XCCMessage>();
                            foreach (var entry in new XFEMultiDictionary(File.ReadAllText($"{groupIdFullPath}/XFEMessage/XFEMessage.xfe")))
                            {
                                var xCCMessage = XCCMessage.ConvertToXCCMessage(entry.Content, groupId);
                                xCCMessageList.Add(xCCMessage);
                                if (xCCMessage.MessageType == XCCTextMessageType.Text)
                                    TextReceived?.Invoke(true, xCCMessage);
                                else
                                    FileReceived?.Invoke(true, LoadFile(xCCMessage));
                            }
                            xCCMessageDictionary.Add(groupId, xCCMessageList);
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(SavePathRoot);
                }
            });
            loaded = true;
        }
        /// <summary>
        /// 从设置的根目录的指定群组加载
        /// </summary>
        /// <param name="groupId">群组ID</param>
        /// <returns></returns>
        public async Task LoadGroup(string groupId)
        {
            await Task.Run(() =>
            {
                if (File.Exists($"{SavePathRoot}/{groupId}/XFEMessage/XFEMessage.xfe"))
                {
                    var xCCMessageList = new List<XCCMessage>();
                    foreach (var entry in new XFEMultiDictionary(File.ReadAllText($"{SavePathRoot}/{groupId}/XFEMessage/XFEMessage.xfe")))
                    {
                        var xCCMessage = XCCMessage.ConvertToXCCMessage(entry.Content, groupId);
                        xCCMessageList.Add(xCCMessage);
                        if (xCCMessage.MessageType == XCCTextMessageType.Text)
                            TextReceived?.Invoke(true, xCCMessage);
                        else
                            FileReceived?.Invoke(true, LoadFile(xCCMessage));
                    }
                    xCCMessageDictionary.Add(groupId, xCCMessageList);
                }
            });
            loaded = true;
        }
        /// <summary>
        /// 清理无用文件
        /// </summary>
        /// <returns></returns>
        public async Task ClearUselessFile()
        {
            if (!loaded)
                throw new XFEExtensionException("不能在加载完成前调用清理");
            await Task.Run(() =>
            {
                foreach (var groupId in xCCMessageDictionary.Keys)
                {
                    foreach (var file in Directory.EnumerateFiles($"{SavePathRoot}/{groupId}"))
                    {
                        var filePath = $"{SavePathRoot}/{groupId}/{file}";
                        var messageId = Path.GetFileNameWithoutExtension(filePath);
                        if (!xCCMessageDictionary.ContainsKey(groupId) || xCCMessageDictionary[groupId].Find(x => x.MessageId == messageId) == null)
                        {
                            File.Delete(filePath);
                        }
                    }
                }
            });
        }
        private XCCFile LoadFile(XCCMessage xCCMessage)
        {
            var filePath = $"{SavePathRoot}/{xCCMessage.GroupId}/{xCCMessage.MessageId}.xfe";
            byte[] fileBuffer = null;
            if (File.Exists(filePath))
                fileBuffer = File.ReadAllBytes(filePath);
            XCCFile xCCFile;
            switch (xCCMessage.MessageType)
            {
                case XCCTextMessageType.Image:
                    xCCFile = new XCCFile(xCCMessage.GroupId, xCCMessage.MessageId, XCCFileType.Image, xCCMessage.Sender, xCCMessage.SendTime, fileBuffer);
                    break;
                case XCCTextMessageType.Audio:
                    xCCFile = new XCCFile(xCCMessage.GroupId, xCCMessage.MessageId, XCCFileType.Audio, xCCMessage.Sender, xCCMessage.SendTime, fileBuffer);
                    break;
                case XCCTextMessageType.Video:
                    xCCFile = new XCCFile(xCCMessage.GroupId, xCCMessage.MessageId, XCCFileType.Video, xCCMessage.Sender, xCCMessage.SendTime, fileBuffer);
                    break;
                default:
                    return null;
            }
            xCCFileDictionary.Add(xCCMessage.MessageId, xCCFile);
            return xCCFile;
        }
        /// <summary>
        /// 获取文件
        /// </summary>
        /// <param name="messageId">消息ID</param>
        /// <returns></returns>
        public XCCFile GetFile(string messageId)
        {
            return xCCFileDictionary.ContainsKey(messageId) ? xCCFileDictionary[messageId] : null;
        }
        /// <summary>
        /// 添加文件
        /// </summary>
        /// <param name="xCCFile">XCC文件实例</param>
        public void AddFile(XCCFile xCCFile)
        {
            xCCFileDictionary.Add(xCCFile.MessageId, xCCFile);
            if (AutoSaveInLocal && xCCFile.FileBuffer != null)
                SaveFile(xCCFile);
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="xCCFile">XCC文件实例</param>
        public void SaveFile(XCCFile xCCFile)
        {
            if (!Directory.Exists($"{SavePathRoot}/{xCCFile.GroupId}"))
            {
                Directory.CreateDirectory($"{SavePathRoot}/{xCCFile.GroupId}");
            }
            File.WriteAllBytes($"{SavePathRoot}/{xCCFile.GroupId}/{xCCFile.MessageId}.xfe", xCCFile.FileBuffer);
        }
        /// <summary>
        /// 保存群组消息
        /// </summary>
        /// <param name="groupId"></param>
        public void SaveMessage(string groupId)
        {
            var filePath = $"{SavePathRoot}/{groupId}/XFEMessage";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            var storageDictionary = new XFEDictionary();
            foreach (var xCCMessage in xCCMessageDictionary[groupId])
            {
                storageDictionary.Add(xCCMessage.MessageId, xCCMessage.ToString());
            }
            File.WriteAllText(filePath + "/XFEMessage.xfe", storageDictionary.ToString());
        }
        private void ReceiveFilePlaceHolder(XCCTextMessageReceivedEventArgs e, XCCFileType fileType)
        {
            var xCCFile = new XCCFile(e.GroupId, e.MessageId, fileType, e.Sender, e.SendTime);
            if (!xCCFileDictionary.ContainsKey(e.MessageId))
            {
                xCCFileDictionary.Add(e.MessageId, xCCFile);
            }
            FileReceived?.Invoke(e.IsHistory, xCCFile);
            if (AutoSaveInLocal)
                SaveMessage(e.GroupId);
        }
        /// <summary>
        /// 接收文本消息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        public void ReceiveTextMessage(object sender, XCCTextMessageReceivedEventArgs e)
        {
            var message = new XCCMessage(e.MessageId, e.MessageType, e.TextMessage, e.Sender, e.SendTime, e.GroupId);
            if (xCCMessageDictionary.ContainsKey(e.GroupId))
            {
                if (xCCMessageDictionary[e.GroupId].Find(x => x.MessageId == e.MessageId) == null)
                {
                    xCCMessageDictionary[e.GroupId].Add(message);
                }
                else
                {
                    return;
                }
            }
            else
            {
                xCCMessageDictionary.Add(e.GroupId, new List<XCCMessage>() { message });
            }
            switch (e.MessageType)
            {
                case XCCTextMessageType.Text:
                    TextReceived?.Invoke(e.IsHistory, message);
                    if (AutoSaveInLocal)
                        SaveMessage(e.GroupId);
                    break;
                case XCCTextMessageType.Image:
                    ReceiveFilePlaceHolder(e, XCCFileType.Image);
                    break;
                case XCCTextMessageType.Audio:
                    ReceiveFilePlaceHolder(e, XCCFileType.Audio);
                    break;
                case XCCTextMessageType.Video:
                    ReceiveFilePlaceHolder(e, XCCFileType.Video);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 接收二进制消息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        public void ReceiveBinaryMessage(object sender, XCCBinaryMessageReceivedEventArgs e)
        {
            if (xCCFileDictionary.ContainsKey(e.MessageId))
            {
                var xCCFile = xCCFileDictionary[e.MessageId];
                if (!xCCFile.Loaded)
                {
                    xCCFileDictionary[e.MessageId].LoadFile(e.BinaryMessage);
                    if (AutoSaveInLocal)
                        SaveFile(xCCFile);
                }
            }
        }
        /// <summary>
        /// XCC消息接收器
        /// </summary>
        /// <param name="savePathRoot">保存根目录</param>
        /// <param name="autoSaveInLocal">自动保存</param>
        public XCCMessageReceiveHelper(string savePathRoot, bool autoSaveInLocal = true)
        {
            AutoSaveInLocal = autoSaveInLocal;
            SavePathRoot = savePathRoot;
        }
        /// <summary>
        /// XCC消息接收器
        /// </summary>
        /// <param name="savePathRoot">保存根目录</param>
        /// <param name="xCCNetWork">XCC网络通讯实例</param>
        /// <param name="autoSaveInLocal">自动保存</param>
        public XCCMessageReceiveHelper(string savePathRoot, XCCNetWork xCCNetWork, bool autoSaveInLocal = true)
        {
            AutoSaveInLocal = autoSaveInLocal;
            SavePathRoot = savePathRoot;
            xCCNetWork.TextMessageReceived += ReceiveTextMessage;
            xCCNetWork.BinaryMessageReceived += ReceiveBinaryMessage;
        }
    }
    /// <summary>
    /// XCC文件
    /// </summary>
    public class XCCFile
    {
        /// <summary>
        /// 文件加载完成时触发
        /// </summary>
        public event XFEEventHandler<XCCFile> FileLoaded;
        /// <summary>
        /// 群组ID
        /// </summary>
        public string GroupId { get; }
        /// <summary>
        /// 消息ID
        /// </summary>
        public string MessageId { get; }
        /// <summary>
        /// 发送者
        /// </summary>
        public string Sender { get; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; }
        /// <summary>
        /// XCC文件类型
        /// </summary>
        public XCCFileType FileType { get; }
        /// <summary>
        /// 是否已加载
        /// </summary>
        public bool Loaded { get; private set; }
        /// <summary>
        /// 文件流
        /// </summary>
        public byte[] FileBuffer { get; set; }
        /// <summary>
        /// 加载文件
        /// </summary>
        /// <param name="fileBuffer">文件流</param>
        public void LoadFile(byte[] fileBuffer)
        {
            FileBuffer = fileBuffer;
            Loaded = true;
            FileLoaded?.Invoke(this);
        }
        /// <summary>
        /// XCC文件
        /// </summary>
        /// <param name="groupId">群组ID</param>
        /// <param name="messageId">文件消息ID</param>
        /// <param name="fileType">文件类型</param>
        /// <param name="sender">发送者</param>
        /// <param name="sendTime">发送时间</param>
        /// <param name="fileBuffer">文件的Buffer</param>
        public XCCFile(string groupId, string messageId, XCCFileType fileType, string sender, DateTime sendTime, byte[] fileBuffer = null)
        {
            GroupId = groupId;
            MessageId = messageId;
            FileType = fileType;
            Sender = sender;
            SendTime = sendTime;
            FileBuffer = fileBuffer;
            Loaded = fileBuffer != null;
        }
    }
    /// <summary>
    /// XCC消息
    /// </summary>
    public class XCCMessage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string MessageId { get; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public XCCTextMessageType MessageType { get; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// 发送者
        /// </summary>
        public string Sender { get; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; }
        /// <summary>
        /// 群组ID
        /// </summary>
        public string GroupId { get; }
        /// <summary>
        /// 封装为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new string[] { MessageId, MessageType.ToString(), Message, Sender, SendTime.ToString() }.ToXFEString();
        }
        /// <summary>
        /// 将封装后的XCC消息字符串转换为XCC消息对象
        /// </summary>
        /// <param name="xCCMessageStringFormat"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static XCCMessage ConvertToXCCMessage(string xCCMessageStringFormat, string groupId)
        {
            var unPackedMessage = xCCMessageStringFormat.ToXFEArray<string>();
            return new XCCMessage(unPackedMessage[0], (XCCTextMessageType)Enum.Parse(typeof(XCCTextMessageType), unPackedMessage[1]), unPackedMessage[2], unPackedMessage[3], DateTime.Parse(unPackedMessage[4]), groupId);
        }
        /// <summary>
        /// XCC消息
        /// </summary>
        /// <param name="messageId">消息ID</param>
        /// <param name="messageType">消息类型</param>
        /// <param name="message">消息内容</param>
        /// <param name="sender">发送者</param>
        /// <param name="sendTime">发送时间</param>
        /// <param name="groupId">群组ID</param>
        public XCCMessage(string messageId, XCCTextMessageType messageType, string message, string sender, DateTime sendTime, string groupId)
        {
            MessageId = messageId;
            MessageType = messageType;
            Message = message;
            Sender = sender;
            SendTime = sendTime;
            GroupId = groupId;
        }
    }
    /// <summary>
    /// XCC网络通讯事件
    /// </summary>
    public abstract class XCCMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// 触发事件的群组
        /// </summary>
        public XCCGroup Group { get; }
        /// <summary>
        /// WebSocket明文传输客户端
        /// </summary>
        public ClientWebSocket TextMessageClientWebSocket { get; private set; }
        /// <summary>
        /// WebSocket文件传输客户端
        /// </summary>
        public ClientWebSocket FileTransportClientWebSocket { get; private set; }
        /// <summary>
        /// XCC服务器连接类型
        /// </summary>
        public XCCClientType XCCClientType { get; }
        /// <summary>
        /// 消息ID
        /// </summary>
        public string MessageId { get; }
        /// <summary>
        /// 群组ID
        /// </summary>
        public string GroupId
        {
            get
            {
                return Group.GroupId;
            }
        }
        /// <summary>
        /// 发送者
        /// </summary>
        public string Sender { get; }
        /// <summary>
        /// 回复文本消息
        /// </summary>
        /// <param name="message">待发送的文本</param>
        /// <returns>发送进程</returns>
        public async Task ReplyTextMessage(string message)
        {
            try
            {
                byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
                await TextMessageClientWebSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("收到服务器端数据后客户端回复文本数据时出现异常", ex);
            }
        }
        /// <summary>
        /// 回复二进制消息
        /// </summary>
        /// <param name="message">二进制消息</param>
        /// <exception cref="XFECyberCommException"></exception>
        public async Task ReplyBinaryMessage(byte[] message)
        {
            try
            {
                await FileTransportClientWebSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("收到服务器端数据后客户端回复文本数据时出现异常", ex);
            }
        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <exception cref="XFECyberCommException"></exception>
        public async Task Close()
        {
            try
            {
                if (TextMessageClientWebSocket.State == WebSocketState.Open)
                    await TextMessageClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端主动关闭连接", CancellationToken.None);
                if (FileTransportClientWebSocket.State == WebSocketState.Open)
                    await FileTransportClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端主动关闭连接", CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("客户端关闭连接时出现异常", ex);
            }
        }
        internal XCCMessageReceivedEventArgs(XCCGroup group, ClientWebSocket textMessageClientWebSocket, ClientWebSocket fileTransportClientWebSocket, XCCClientType xCCClientType, string sender, string messageId)
        {
            Group = group;
            TextMessageClientWebSocket = textMessageClientWebSocket;
            FileTransportClientWebSocket = fileTransportClientWebSocket;
            XCCClientType = xCCClientType;
            Sender = sender;
            MessageId = messageId;
        }
    }
    /// <summary>
    /// XFE网络通信明文返回消息类型
    /// </summary>
    public enum XCCTextMessageType
    {
        /// <summary>
        /// 文本消息
        /// </summary>
        Text,
        /// <summary>
        /// 图片消息
        /// </summary>
        Image,
        /// <summary>
        /// 音频消息
        /// </summary>
        Audio,
        /// <summary>
        /// 视频消息
        /// </summary>
        Video
    }
    /// <summary>
    /// XCC网络通讯接收到明文消息事件
    /// </summary>
    public abstract class XCCTextMessageReceivedEventArgs : XCCMessageReceivedEventArgs
    {
        /// <summary>
        /// 返回文本消息类型
        /// </summary>
        public XCCTextMessageType MessageType { get; }
        /// <summary>
        /// 文本消息
        /// </summary>
        public string TextMessage { get; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; }
        /// <summary>
        /// 是否为历史消息
        /// </summary>
        public bool IsHistory { get; }
        internal XCCTextMessageReceivedEventArgs(XCCGroup group, ClientWebSocket textMessageClientWebSocket, ClientWebSocket fileTransportClientWebSocket, XCCClientType xCCClientType, string messageId, XCCTextMessageType messageType, string message, string sender, DateTime sendTime, bool isHistory) : base(group, textMessageClientWebSocket, fileTransportClientWebSocket, xCCClientType, sender, messageId)
        {
            MessageType = messageType;
            TextMessage = message;
            SendTime = sendTime;
            IsHistory = isHistory;
        }

    }
    /// <summary>
    /// XFE网络通信二进制返回消息类型
    /// </summary>
    public enum XCCBinaryMessageType
    {
        /// <summary>
        /// 文本消息
        /// </summary>
        Text,
        /// <summary>
        /// 二进制消息
        /// </summary>
        Binary,
        /// <summary>
        /// 图片消息
        /// </summary>
        Image,
        /// <summary>
        /// 音频消息
        /// </summary>
        Audio,
        /// <summary>
        /// 实时音频
        /// </summary>
        AudioBuffer,
        /// <summary>
        /// 视频消息
        /// </summary>
        Video
    }
    /// <summary>
    /// XCC网络通讯接收到二进制消息事件
    /// </summary>
    public abstract class XCCBinaryMessageReceivedEventArgs : XCCMessageReceivedEventArgs
    {
        /// <summary>
        /// 返回二进制消息类型
        /// </summary>
        public XCCBinaryMessageType MessageType { get; }
        /// <summary>
        /// 消息签名
        /// </summary>
        public string Signature { get; }
        /// <summary>
        /// 二进制消息
        /// </summary>
        public byte[] BinaryMessage { get; }
        internal XCCBinaryMessageReceivedEventArgs(XCCGroup group, ClientWebSocket textMessageClientWebSocket, ClientWebSocket fileTransportClientWebSocket, XCCClientType xCCClientType, string sender, string messageId, byte[] buffer, XCCBinaryMessageType messageType, string signature) : base(group, textMessageClientWebSocket, fileTransportClientWebSocket, xCCClientType, sender, messageId)
        {
            BinaryMessage = buffer;
            MessageType = messageType;
            Signature = signature;
        }
    }
    /// <summary>
    /// XCC网络通讯期间异常事件
    /// </summary>
    public abstract class XCCExceptionMessageReceivedEventArgs : XCCMessageReceivedEventArgs
    {
        /// <summary>
        /// 异常信息
        /// </summary>
        public XFECyberCommException Exception { get; }
        internal XCCExceptionMessageReceivedEventArgs(XCCGroup group, ClientWebSocket textMessageClientWebSocket, ClientWebSocket fileTransportClientWebSocket, XCCClientType xCCClientType, string sender, string messageId, XFECyberCommException exception) : base(group, textMessageClientWebSocket, fileTransportClientWebSocket, xCCClientType, sender, messageId)
        {
            Exception = exception;
        }
    }
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
        public ClientWebSocket TextMessageClientWebSocket { get; private set; }
        /// <summary>
        /// WebSocket文件传输客户端
        /// </summary>
        public ClientWebSocket FileTransportClientWebSocket { get; private set; }
        /// <summary>
        /// 是否正常关闭
        /// </summary>
        public bool ClosedNormally { get; }

        internal XCCConnectionClosedEventArgs(XCCGroup group, XCCClientType xCCClientType, ClientWebSocket textMessageClientWebSocket, ClientWebSocket fileTransportClientWebSocket, bool closedNormally)
        {
            Group = group;
            XCCClientType = xCCClientType;
            TextMessageClientWebSocket = textMessageClientWebSocket;
            FileTransportClientWebSocket = fileTransportClientWebSocket;
            ClosedNormally = closedNormally;
        }
    }
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
        public ClientWebSocket TextMessageClientWebSocket { get; private set; }
        /// <summary>
        /// WebSocket文件传输客户端
        /// </summary>
        public ClientWebSocket FileTransportClientWebSocket { get; private set; }

        internal XCCConnectedEventArgs(XCCGroup group, XCCClientType xCCClientType, ClientWebSocket textMessageClientWebSocket, ClientWebSocket fileTransportClientWebSocket)
        {
            Group = group;
            XCCClientType = xCCClientType;
            TextMessageClientWebSocket = textMessageClientWebSocket;
            FileTransportClientWebSocket = fileTransportClientWebSocket;
        }
    }
    class XCCGroupImpl : XCCGroup
    {
        internal XCCGroupImpl(string groupId, string sender, XCCNetWorkBase xCCNetWorkBase) : base(groupId, sender, xCCNetWorkBase) { }
    }
    class XCCConnectionClosedEventArgsImpl : XCCConnectionClosedEventArgs
    {
        public XCCConnectionClosedEventArgsImpl(XCCGroup group, XCCClientType xCCClientType, ClientWebSocket textMessageClientWebSocket, ClientWebSocket fileTransportClientWebSocket, bool closedNormally) : base(group, xCCClientType, textMessageClientWebSocket, fileTransportClientWebSocket, closedNormally) { }
    }
    class XCCConnectedEventArgsImpl : XCCConnectedEventArgs
    {
        public XCCConnectedEventArgsImpl(XCCGroup group, XCCClientType xCCClientType, ClientWebSocket textMessageClientWebSocket, ClientWebSocket fileTransportClientWebSocket) : base(group, xCCClientType, textMessageClientWebSocket, fileTransportClientWebSocket) { }
    }
    class XCCTextMessageReceivedEventArgsImpl : XCCTextMessageReceivedEventArgs
    {
        internal XCCTextMessageReceivedEventArgsImpl(XCCGroup group, ClientWebSocket textMessageClientWebSocket, ClientWebSocket fileTransportClientWebSocket, XCCClientType xCCClientType, string messageId, XCCTextMessageType messageType, string message, string sender, DateTime sendTime, bool isHistory) : base(group, textMessageClientWebSocket, fileTransportClientWebSocket, xCCClientType, messageId, messageType, message, sender, sendTime, isHistory) { }
    }
    class XCCBinaryMessageReceivedEventArgsImpl : XCCBinaryMessageReceivedEventArgs
    {
        internal XCCBinaryMessageReceivedEventArgsImpl(XCCGroup group, ClientWebSocket textMessageClientWebSocket, ClientWebSocket fileTransportClientWebSocket, XCCClientType xCCClientType, string sender, string messageId, byte[] buffer, XCCBinaryMessageType messageType, string signature) : base(group, textMessageClientWebSocket, fileTransportClientWebSocket, xCCClientType, sender, messageId, buffer, messageType, signature) { }
    }
    class XCCExceptionMessageReceivedEventArgsImpl : XCCExceptionMessageReceivedEventArgs
    {
        internal XCCExceptionMessageReceivedEventArgsImpl(XCCGroup group, ClientWebSocket textMessageClientWebSocket, ClientWebSocket fileTransportClientWebSocket, XCCClientType xCCClientType, string sender, string messageId, XFECyberCommException exception) : base(group, textMessageClientWebSocket, fileTransportClientWebSocket, xCCClientType, sender, messageId, exception) { }
    }
}