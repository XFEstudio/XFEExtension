using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XFE各类拓展.ArrayExtension;
using XFE各类拓展.ArrayExtension.AI;
using XFE各类拓展.BufferExtension;
using XFE各类拓展.FormatExtension;
using XFE各类拓展.TaskExtension;

namespace XFE各类拓展.CyberComm
{
    /// <summary>
    /// 传回的参数类型
    /// </summary>
    public enum BackMessageType
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
        /// 错误消息
        /// </summary>
        Error
    }
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
        public string ServerURL { get; set; }
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
        public event EventHandler<CyberCommClientEventArgs> MessageReceived;
        /// <summary>
        /// 连接关闭时触发
        /// </summary>
        public event EventHandler ConnectionClosed;
        /// <summary>
        /// 连接成功时触发
        /// </summary>
        public event EventHandler Connected;
        /// <summary>
        /// WebSocket客户端
        /// </summary>
        public ClientWebSocket ClientWebSocket { get; private set; }
        #endregion
        #region 公有方法
        /// <summary>
        /// 启动CyberComm客户端
        /// </summary>
        /// <returns></returns>
        public async void StartCyberCommClient()
        {
        StartConnect:
            ClientWebSocket = new ClientWebSocket();
            Uri serverUri = new Uri(ServerURL);
            reconnectTimes++;
            try
            {
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
                        MessageReceived?.Invoke(this, new CyberCommClientEventArgsImpl(ClientWebSocket, receivedMessage));
                    }
                    if (receiveResult.MessageType == WebSocketMessageType.Binary)
                    {

                        MessageReceived?.Invoke(this, new CyberCommClientEventArgsImpl(ClientWebSocket, receivedBinaryBuffer));
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
        public async void SendTextMessage(string message)
        {
            try
            {
                byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
                await ClientWebSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
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
        public async void SendBinaryMessage(byte[] message)
        {
            try
            {
                await ClientWebSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Binary, true, CancellationToken.None);
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
        public async void CloseCyberCommClient()
        {
            await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
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
            this.AutoReconnect = autoReconnect;
            this.AutoReceiveCompletedMessage = autoReceiveCompletedMessage;
            this.ServerURL = serverURL;
        }
        /// <summary>
        /// CyberComm客户端
        /// </summary>
        public CyberCommClient() { }
        #endregion
    }
    /// <summary>
    /// CyberComm服务器
    /// </summary>
    public class CyberCommServer
    {
        private readonly string serverURL;
        #region 公共属性
        /// <summary>
        /// 是否自动接收完整消息
        /// </summary>
        public bool AutoReceiveCompletedMessage { get; set; }
        /// <summary>
        /// 收到消息时触发
        /// </summary>
        public event EventHandler<CyberCommServerEventArgs> MessageReceived;
        /// <summary>
        /// 客户端连接时触发
        /// </summary>
        public event EventHandler<CyberCommServerEventArgs> ClientConnected;
        /// <summary>
        /// 服务器启动时触发
        /// </summary>
        public event EventHandler ServerStarted;
        /// <summary>
        /// 连接关闭时触发
        /// </summary>
        public event EventHandler<CyberCommServerEventArgs> ConnectionClosed;
        /// <summary>
        /// WebSocket服务器
        /// </summary>
        public HttpListener WebSocketServer { get; } = new HttpListener();
        #endregion
        #region 公有方法
        /// <summary>
        /// 启动CyberComm服务器
        /// </summary>
        /// <returns></returns>
        public async void StartCyberCommServer()
        {
            try
            {
                WebSocketServer.Prefixes.Add(serverURL);
                WebSocketServer.Start();
                ServerStarted?.Invoke(this, EventArgs.Empty);
                while (true)
                {
                    HttpListenerContext httpListenerContext = await WebSocketServer.GetContextAsync();
                    if (httpListenerContext.Request.IsWebSocketRequest)
                    {
                        HttpListenerWebSocketContext httpListenerWebSocketContext = await httpListenerContext.AcceptWebSocketAsync(null);
                        string clientIP = httpListenerContext.Request.RemoteEndPoint.Address.ToString();
                        WebSocket webSocket = httpListenerWebSocketContext.WebSocket;
                        NameValueCollection wsHeader = httpListenerWebSocketContext.Headers;
                        ClientConnected?.Invoke(this, new CyberCommServerEventArgsImpl(webSocket, string.Empty, clientIP, wsHeader));
                        CyberCommClientConnected(webSocket, wsHeader, clientIP);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("启动服务器时发生异常", ex);
            }
        }
        private async void CyberCommClientConnected(WebSocket webSocket, NameValueCollection wsHeader, string clientIP)
        {
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    byte[] receiveBuffer = new byte[1024];
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    //ReceiveCompletedMessageByUsingWhile
                    var bufferList = new List<byte>();
                    if (AutoReceiveCompletedMessage)
                    {
                        bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                        while (!receiveResult.EndOfMessage)
                        {
                            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                            bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                        }
                    }
                    var receivedBinaryBuffer = bufferList.ToArray();
                    if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(receivedBinaryBuffer);
                        MessageReceived?.Invoke(this, new CyberCommServerEventArgsImpl(webSocket, receivedMessage, clientIP, wsHeader));
                    }
                    if (receiveResult.MessageType == WebSocketMessageType.Binary)
                    {
                        MessageReceived?.Invoke(this, new CyberCommServerEventArgsImpl(webSocket, receivedBinaryBuffer, clientIP, wsHeader));
                    }
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection Closed", CancellationToken.None);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    MessageReceived?.Invoke(this, new CyberCommServerEventArgsImpl(webSocket, new XFECyberCommException("与客户端端通讯期间发生异常", ex), clientIP, wsHeader));
                    break;
                }
            }
            ConnectionClosed?.Invoke(this, new CyberCommServerEventArgsImpl(webSocket, string.Empty, clientIP, wsHeader));
            webSocket.Dispose();
        }
        #endregion
        #region 构造函数
        /// <summary>
        /// CyberComm服务器，使用端口创建
        /// </summary>
        /// <param name="ListenPort">监听端口</param>
        /// <param name="AutoReceiveCompletedMessage">是否自动接收完整消息</param>
        public CyberCommServer(int ListenPort, bool AutoReceiveCompletedMessage = true)
        {
            this.serverURL = $"http://*:{ListenPort}/";
            this.AutoReceiveCompletedMessage = AutoReceiveCompletedMessage;
        }
        /// <summary>
        /// CyberComm服务器，使用URL创建
        /// </summary>
        /// <param name="ServerURL">服务器URL</param>
        /// <param name="AutoReceiveCompletedMessage">是否自动接收完整消息</param>
        public CyberCommServer(string ServerURL, bool AutoReceiveCompletedMessage = true)
        {
            this.serverURL = ServerURL;
            this.AutoReceiveCompletedMessage = AutoReceiveCompletedMessage;
        }
        #endregion
    }
    #region 事件参数及其实现类
    /// <summary>
    /// CyberComm客户端事件参数
    /// </summary>
    public abstract class CyberCommClientEventArgs : EventArgs
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public BackMessageType MessageType { get; }
        /// <summary>
        /// 当前WebSocket
        /// </summary>
        public ClientWebSocket CurrentWebSocket { get; }
        /// <summary>
        /// 文本消息
        /// </summary>
        public string TextMessage { get; }
        /// <summary>
        /// 异常消息（如果有的话）
        /// </summary>
        public XFECyberCommException Exception { get; }
        /// <summary>
        /// 二进制消息
        /// </summary>
        public byte[] BinaryMessage { get; }
        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="message">待发送的文本</param>
        /// <exception cref="XFECyberCommException"></exception>
        /// <returns>发送进程</returns>
        public async void ReplyMessage(string message)
        {
            try
            {
                byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
                await CurrentWebSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
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
        public async void Close()
        {
            try
            {
                await CurrentWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection Closed", CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("关闭客户端连接时出现异常", ex);
            }
        }
        internal CyberCommClientEventArgs(ClientWebSocket clientWebSocket, string message)
        {
            CurrentWebSocket = clientWebSocket;
            TextMessage = message;
            MessageType = BackMessageType.Text;
        }
        internal CyberCommClientEventArgs(ClientWebSocket clientWebSocket, byte[] bytes)
        {
            CurrentWebSocket = clientWebSocket;
            BinaryMessage = bytes;
            MessageType = BackMessageType.Binary;
        }
        internal CyberCommClientEventArgs(ClientWebSocket clientWebSocket, XFECyberCommException ex)
        {
            CurrentWebSocket = clientWebSocket;
            Exception = ex;
            MessageType = BackMessageType.Error;
        }
    }
    /// <summary>
    /// CyberComm服务器事件参数
    /// </summary>
    public abstract class CyberCommServerEventArgs
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public BackMessageType MessageType { get; }
        /// <summary>
        /// 当前WebSocket
        /// </summary>
        public WebSocket CurrentWebSocket { get; }
        /// <summary>
        /// 客户端请求头
        /// </summary>
        public NameValueCollection WSHeader { get; }
        /// <summary>
        /// 异常消息（如果有的话）
        /// </summary>
        public XFECyberCommException Exception { get; }
        /// <summary>
        /// 客户端IP地址
        /// </summary>
        public string IpAddress { get; }
        /// <summary>
        /// 文本消息
        /// </summary>
        public string TextMessage { get; }
        /// <summary>
        /// 二进制消息
        /// </summary>
        public byte[] BinaryMessage { get; }
        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="message">待发送的文本</param>
        /// <exception cref="XFECyberCommException"></exception>
        /// <returns>发送进程</returns>
        public async void ReplyMessage(string message)
        {
            try
            {
                byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
                await CurrentWebSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("收到客户端数据后服务器端回复文本时出现异常", ex);
            }
        }
        /// <summary>
        /// 发送二进制消息
        /// </summary>
        /// <param name="bytes">二进制消息</param>
        /// <exception cref="XFECyberCommException"></exception>
        /// <returns></returns>
        public async void ReplyBinaryMessage(byte[] bytes)
        {
            try
            {
                await CurrentWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("收到客户端数据后服务器端回复二进制数据时出现异常", ex);
            }
        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <exception cref="XFECyberCommException"></exception>
        public async void Close()
        {
            try
            {
                await CurrentWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection Closed", CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("关闭服务器端连接时出现异常", ex);
            }
        }
        /// <summary>
        /// 强制关闭连接
        /// </summary>
        /// <exception cref="XFECyberCommException"></exception>
        public void ForceClose()
        {
            try
            {
                CurrentWebSocket.Abort();
            }
            catch (Exception ex)
            {
                throw new XFECyberCommException("强制关闭服务器端连接时出现异常", ex);
            }
        }
        internal CyberCommServerEventArgs(WebSocket webSocket, string message, string ipAddress, NameValueCollection wsHeader)
        {
            CurrentWebSocket = webSocket;
            TextMessage = message;
            IpAddress = ipAddress;
            MessageType = BackMessageType.Text;
            WSHeader = wsHeader;
        }
        internal CyberCommServerEventArgs(WebSocket webSocket, byte[] bytes, string ipAddress, NameValueCollection wsHeader)
        {
            CurrentWebSocket = webSocket;
            BinaryMessage = bytes;
            IpAddress = ipAddress;
            MessageType = BackMessageType.Binary;
            WSHeader = wsHeader;
        }
        internal CyberCommServerEventArgs(WebSocket webSocket, XFECyberCommException ex, string ipAddress, NameValueCollection wsHeader)
        {
            CurrentWebSocket = webSocket;
            Exception = ex;
            IpAddress = ipAddress;
            MessageType = BackMessageType.Error;
            WSHeader = wsHeader;
        }
    }
    class CyberCommClientEventArgsImpl : CyberCommClientEventArgs
    {
        internal CyberCommClientEventArgsImpl(ClientWebSocket clientWebSocket, string message) : base(clientWebSocket, message) { }
        internal CyberCommClientEventArgsImpl(ClientWebSocket clientWebSocket, byte[] bytes) : base(clientWebSocket, bytes) { }
        internal CyberCommClientEventArgsImpl(ClientWebSocket clientWebSocket, XFECyberCommException ex) : base(clientWebSocket, ex) { }
    }
    class CyberCommServerEventArgsImpl : CyberCommServerEventArgs
    {
        internal CyberCommServerEventArgsImpl(WebSocket webSocket, string message, string ipAddress, NameValueCollection wsHeader) : base(webSocket, message, ipAddress, wsHeader) { }
        internal CyberCommServerEventArgsImpl(WebSocket webSocket, byte[] bytes, string ipAddress, NameValueCollection wsHeader) : base(webSocket, bytes, ipAddress, wsHeader) { }
        internal CyberCommServerEventArgsImpl(WebSocket webSocket, XFECyberCommException ex, string ipAddress, NameValueCollection wsHeader) : base(webSocket, ex, ipAddress, wsHeader) { }
    }
    #endregion
    /// <summary>
    /// CyberComm客户端群组
    /// </summary>
    public class CyberCommGroup
    {
        private readonly List<WebSocket> webSockets = new List<WebSocket>();
        /// <summary>
        /// 组ID
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// 添加客户端
        /// </summary>
        /// <param name="webSocket">客户端</param>
        public void Add(WebSocket webSocket)
        {
            webSockets.Add(webSocket);
        }
        /// <summary>
        /// 获取指定的客户端
        /// </summary>
        /// <param name="webSocket">客户端</param>
        public void Remove(WebSocket webSocket)
        {
            webSockets.Remove(webSocket);
        }
        /// <summary>
        /// 移除指定索引的客户端
        /// </summary>
        /// <param name="index">客户单索引</param>
        public void RemoveAt(int index)
        {
            webSockets.RemoveAt(index);
        }
        /// <summary>
        /// 清空列表
        /// </summary>
        public void Clear()
        {
            webSockets.Clear();
        }
        /// <summary>
        /// 群组客户端数量
        /// </summary>
        public int Count
        {
            get
            {
                return webSockets.Count;
            }
        }
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>客户端</returns>
        public WebSocket this[int index]
        {
            get
            {
                return webSockets[index];
            }
        }
        /// <summary>
        /// 发送群组文本消息
        /// </summary>
        /// <param name="message">群发文本消息</param>
        public void SendGroupTextMessage(string message)
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
            foreach (WebSocket webSocket in webSockets)
            {
                webSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
        /// <summary>
        /// 向除了指定的WS客户端外的客户端发送群组文本消息
        /// </summary>
        /// <param name="message">群发文本消息</param>
        /// <param name="exceptWebSocket">除了指定的WS客户端外的客户端</param>
        public void SendGroupTextMessageExcept(string message, WebSocket exceptWebSocket)
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
            foreach (WebSocket webSocket in webSockets)
            {
                if (webSocket != exceptWebSocket)
                {
                    webSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
        /// <summary>
        /// 发送群组二进制消息
        /// </summary>
        /// <param name="bytes">群发二进制消息</param>
        public void SendGroupBinaryMessage(byte[] bytes)
        {
            foreach (WebSocket webSocket in webSockets)
            {
                webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
        /// <summary>
        /// 向除了指定的WS客户端外的客户端发送群组二进制消息
        /// </summary>
        /// <param name="bytes">群发二进制消息</param>
        /// <param name="exceptWebSocket">除了指定的WS客户端外的客户端</param>
        public void SendGroupBinaryMessageExcept(byte[] bytes, WebSocket exceptWebSocket)
        {
            foreach (WebSocket webSocket in webSockets)
            {
                if (webSocket != exceptWebSocket)
                {
                    webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            }
        }
        /// <summary>
        /// 客户端群组
        /// </summary>
        /// <param name="GroupId">群组ID</param>
        public CyberCommGroup(string GroupId)
        {
            this.GroupId = GroupId;
        }
        /// <summary>
        /// 客户端群组
        /// </summary>
        /// <param name="GroupId">群组ID</param>
        /// <param name="webSockets">客户端群组</param>
        public CyberCommGroup(string GroupId, List<WebSocket> webSockets)
        {
            this.GroupId = GroupId;
            this.webSockets = webSockets;
        }
    }
    /// <summary>
    /// 通信群组控制器
    /// </summary>
    public class CyberCommGroupController
    {
        private readonly List<CyberCommGroup> commGroups;
        /// <summary>
        /// 刷新
        /// </summary>
        public void Refresh()
        {
            for (int i = commGroups.Count - 1; i >= 0; i--)
            {
                if (commGroups[i].Count == 0)
                {
                    commGroups.Remove(commGroups[i]);
                }
            }
        }
        /// <summary>
        /// 添加群组
        /// </summary>
        /// <param name="commGroup"></param>
        public void AddGroup(CyberCommGroup commGroup)
        {
            commGroups.Add(commGroup);
        }
        /// <summary>
        /// 移除群组
        /// </summary>
        /// <param name="commGroup"></param>
        public void RemoveGroup(CyberCommGroup commGroup)
        {
            commGroups.Remove(commGroup);
        }
        /// <summary>
        /// 移除指定索引的群组
        /// </summary>
        /// <param name="index"></param>
        public void RemoveGroupAt(int index)
        {
            commGroups.RemoveAt(index);
        }
        /// <summary>
        /// 清空群组
        /// </summary>
        public void Clear()
        {
            commGroups.Clear();
        }
        /// <summary>
        /// 群组数量
        /// </summary>
        public int Count
        {
            get
            {
                return commGroups.Count;
            }
        }
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CyberCommGroup this[int index]
        {
            get
            {
                return commGroups[index];
            }
        }
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="GroupId"></param>
        /// <returns></returns>
        public CyberCommGroup this[string GroupId]
        {
            get
            {
                foreach (CyberCommGroup commGroup in commGroups)
                {
                    if (commGroup.GroupId == GroupId)
                    {
                        return commGroup;
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// 通信群组控制器发送文本消息
        /// </summary>
        /// <param name="GroupId">目标群组的ID</param>
        /// <param name="message">发送的文本消息</param>
        public void SendGroupTextMessage(string GroupId, string message)
        {
            CyberCommGroup commGroup = this[GroupId];
            if (commGroup != null)
            {
                commGroup.SendGroupTextMessage(message);
            }
        }
        /// <summary>
        /// 通信群组控制器发送二进制消息
        /// </summary>
        /// <param name="GroupId">目标群组的ID</param>
        /// <param name="bytes">发送的二进制消息</param>
        public void SendGroupBinaryMessage(string GroupId, byte[] bytes)
        {
            CyberCommGroup commGroup = this[GroupId];
            if (commGroup != null)
            {
                commGroup.SendGroupBinaryMessage(bytes);
            }
        }
        /// <summary>
        /// 通信群组控制器
        /// </summary>
        public CyberCommGroupController()
        {
            commGroups = new List<CyberCommGroup>();
        }
    }
    namespace XCCNetWork
    {
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
            Audio
        }
        /// <summary>
        /// XFE网络通信明文返回消息类型
        /// </summary>
        public enum XCCTextMessageType
        {
            /// <summary>
            /// 文本消息
            /// </summary>
            Text
        }
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
            /// WebSocket客户端
            /// </summary>
            public ClientWebSocket ClientWebSocket { get; private set; }
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
            public async void StartXCC(bool autoReconnect = true, int reconnectMaxTimes = -1, int reconnectTryDelay = 100)
            {
            XCCReconnect:
                ClientWebSocket = new ClientWebSocket();
                Uri serverUri = new Uri("ws://xcc.api.xfegzs.com");
                var base64GroupId = Convert.ToBase64String(Encoding.UTF8.GetBytes(GroupId));
                var base64SenderId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Sender));
                ClientWebSocket.Options.SetRequestHeader("Group", base64GroupId);
                ClientWebSocket.Options.SetRequestHeader("Sender", base64SenderId);
                reconnectTimes++;
                try
                {
                    await ClientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    if (IsConnected == true)
                    {
                        workBase.connectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, ClientWebSocket, false));
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
                        workBase.exceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, ClientWebSocket, null, null, new XFECyberCommException("与XCC网络通讯服务器建立连接时发生异常", ex)));
                        return;
                    }
                }
                reconnectTimes = 0;
                workBase.connected?.Invoke(this, new XCCConnectedEventArgsImpl(this, ClientWebSocket));
                IsConnected = true;
                while (ClientWebSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        byte[] receiveBuffer = new byte[1024];
                        WebSocketReceiveResult receiveResult = await ClientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                        var bufferList = new List<byte>();
                        bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                        //ReceiveCompletedMessageByUsingWhile
                        while (!receiveResult.EndOfMessage)
                        {
                            receiveResult = await ClientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                            bufferList.AddRange(receiveBuffer.Take(receiveResult.Count));
                        }
                        var receivedBinaryBuffer = bufferList.ToArray();
                        if (receiveResult.MessageType == WebSocketMessageType.Text)
                        {
                            try
                            {
                                var receivedMessage = Encoding.UTF8.GetString(receivedBinaryBuffer);
                                var unPackedMessage = receivedMessage.ToXFEArray<string>();
                                workBase.textMessageReceived?.Invoke(this, new XCCTextMessageReceivedEventArgsImpl(this, ClientWebSocket, unPackedMessage[2], unPackedMessage[0], XCCTextMessageType.Text, unPackedMessage[1]));
                            }
                            catch (Exception ex)
                            {
                                workBase.exceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, ClientWebSocket, null, null, new XFECyberCommException("接收XCC服务器消息时发生异常", ex)));
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
                                    case "callback":
                                        UpdateTaskTrigger.Invoke(true, messageId);
                                        continue;
                                    default:
                                        messageType = XCCBinaryMessageType.Binary;
                                        break;
                                }
                                workBase.binaryMessageReceived?.Invoke(this, new XCCBinaryMessageReceivedEventArgsImpl(this, ClientWebSocket, sender, messageId, unPackedBuffer, messageType, signature));
                            }
                            catch (Exception ex)
                            {
                                workBase.exceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, ClientWebSocket, null, null, new XFECyberCommException("接收XCC服务器消息时发生异常", ex)));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        try { await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None); } catch { }
                        if (IsConnected == true)
                        {
                            workBase.connectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, ClientWebSocket, false));
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
                            workBase.exceptionMessageReceived?.Invoke(this, new XCCExceptionMessageReceivedEventArgsImpl(this, ClientWebSocket, null, null, new XFECyberCommException("与XCC网络通讯服务器建立连接时发生异常", ex)));
                            return;
                        }
                    }
                }
                workBase.connectionClosed?.Invoke(this, new XCCConnectionClosedEventArgsImpl(this, ClientWebSocket, true));
            }
            /// <summary>
            /// 发送文本消息，返回消息ID
            /// </summary>
            /// <param name="message">待发送的文本</param>
            /// <returns>消息ID</returns>
            public async Task<string> SendTextMessage(string message)
            {
                var messageId = Guid.NewGuid().ToString();
                await SendTextMessage(message, messageId);
                return messageId;
            }
            /// <summary>
            /// 发送文本消息
            /// </summary>
            /// <param name="message">待发送的文本</param>
            /// <param name="messageId">消息ID</param>
            /// <exception cref="XFECyberCommException"></exception>
            /// <returns>服务器接收校验等待</returns>
            public async Task SendTextMessage(string message, string messageId)
            {
                try
                {
                    byte[] sendBuffer = Encoding.UTF8.GetBytes(new string[] { messageId, message }.ToXFEString());
                    await ClientWebSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    await new XFEWaitTask<bool>(ref UpdateTaskTrigger, messageId);
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
            /// <returns>消息ID</returns>
            [Obsolete("发送者已统一，请使用SendTextMessage或SendBinaryTextMessage")]
            public async Task<string> SendStandardTextMessage(string role, string message)
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
            /// <returns></returns>
            public async Task<string> SendSignedBinaryMessage(byte[] message, string signature)
            {
                var messageId = Guid.NewGuid().ToString();
                await SendSignedBinaryMessage(message, messageId, signature);
                return messageId;
            }
            /// <summary>
            /// 发送签名二进制消息
            /// </summary>
            /// <param name="message">二进制消息</param>
            /// <param name="messageId">消息ID</param>
            /// <param name="signature">签名标识</param>
            /// <returns>服务器接收校验等待</returns>
            /// <exception cref="XFECyberCommException"></exception>
            public async Task SendSignedBinaryMessage(byte[] message, string messageId, string signature)
            {
                try
                {
                    var xFEBuffer = new XFEBuffer(Sender, message, "Type", Encoding.UTF8.GetBytes(signature), "ID", Encoding.UTF8.GetBytes(messageId));
                    await ClientWebSocket.SendAsync(new ArraySegment<byte>(xFEBuffer.ToBuffer()), WebSocketMessageType.Binary, true, CancellationToken.None);
                    await new XFEWaitTask<bool>(ref UpdateTaskTrigger, messageId);
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
            /// <returns>消息ID</returns>
            /// <exception cref="XFECyberCommException"></exception>
            public async Task<string> SendBinaryTextMessage(string message)
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
            /// <exception cref="XFECyberCommException"></exception>
            /// <returns>消息ID</returns>
            public async Task<string> SendBinaryMessage(byte[] message)
            {
                try
                {
                    return await SendSignedBinaryMessage(message, "binary");
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
            /// <returns>消息ID</returns>
            /// <exception cref="XFECyberCommException"></exception>
            public async Task<string> SendImage(string filePath)
            {
                try
                {
                    return await SendSignedBinaryMessage(File.ReadAllBytes(filePath), "image");
                }
                catch (Exception ex)
                {
                    throw new XFECyberCommException("客户端发送图片到服务器时出现异常", ex);
                }
            }
            /// <summary>
            /// 发送音频
            /// </summary>
            /// <param name="buffer">二进制音频流</param>
            /// <returns>消息ID</returns>
            /// <exception cref="XFECyberCommException"></exception>
            public async Task<string> SendAudioBuffer(byte[] buffer)
            {
                try
                {
                    return await SendSignedBinaryMessage(buffer, "audio");
                }
                catch (Exception ex) { throw new XFECyberCommException("客户端发送音频到服务器时出现异常", ex); }
            }
            /// <summary>
            /// 获取历史记录
            /// </summary>
            /// <exception cref="XFECyberCommException"></exception>
            public async Task GetHistory()
            {
                try
                {
                    byte[] sendBuffer = Encoding.UTF8.GetBytes("[XCCGetHistory]");
                    await ClientWebSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
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
                    await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端主动关闭连接", CancellationToken.None);
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
        /// XCC消息
        /// </summary>
        public class XCCMessage : XFEEntry
        {
            /// <summary>
            /// XCC消息
            /// </summary>
            /// <param name="Header">XCC头</param>
            /// <param name="Content">内容</param>
            public XCCMessage(string Header, string Content) : base(Header, Content) { }
        }
        /// <summary>
        /// XCC网络通讯事件
        /// </summary>
        public abstract class XCCMessageReceivedEventArgs : EventArgs
        {
            /// <summary>
            /// 当前WebSocket
            /// </summary>
            public ClientWebSocket CurrentWebSocket { get; }
            /// <summary>
            /// 触发事件的群组
            /// </summary>
            public XCCGroup Group { get; }
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
            public async void ReplyTextMessage(string message)
            {
                try
                {
                    byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
                    await CurrentWebSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
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
            public async void ReplyBinaryMessage(byte[] message)
            {
                try
                {
                    await CurrentWebSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Binary, true, CancellationToken.None);
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
            public async void Close()
            {
                try
                {
                    await CurrentWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端主动关闭连接", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    throw new XFECyberCommException("客户端关闭连接时出现异常", ex);
                }
            }
            internal XCCMessageReceivedEventArgs(XCCGroup group, ClientWebSocket clientWebSocket, string sender, string messageId)
            {
                Group = group;
                CurrentWebSocket = clientWebSocket;
                Sender = sender;
                MessageId = messageId;
            }
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
            internal XCCTextMessageReceivedEventArgs(XCCGroup group, ClientWebSocket clientWebSocket, string sender, string messageId, XCCTextMessageType messageType, string message) : base(group, clientWebSocket, sender, messageId)
            {
                MessageType = messageType;
                TextMessage = message;
            }

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
            internal XCCBinaryMessageReceivedEventArgs(XCCGroup group, ClientWebSocket clientWebSocket, string sender, string messageId, byte[] buffer, XCCBinaryMessageType messageType, string signature) : base(group, clientWebSocket, sender, messageId)
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
            internal XCCExceptionMessageReceivedEventArgs(XCCGroup group, ClientWebSocket clientWebSocket, string sender, string messageId, XFECyberCommException exception) : base(group, clientWebSocket, sender, messageId)
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
            /// 当前WebSocket
            /// </summary>
            public ClientWebSocket CurrentWebSocket { get; }
            /// <summary>
            /// 是否正常关闭
            /// </summary>
            public bool ClosedNormally { get; }
            /// <summary>
            /// 触发事件的群组
            /// </summary>
            public XCCGroup Group { get; }
            internal XCCConnectionClosedEventArgs(XCCGroup group, ClientWebSocket clientWebSocket, bool closeNormally)
            {
                CurrentWebSocket = clientWebSocket;
                Group = group;
                ClosedNormally = closeNormally;
            }
        }
        /// <summary>
        /// XCC连接事件
        /// </summary>
        public abstract class XCCConnectedEventArgs : EventArgs
        {
            /// <summary>
            /// 当前WebSocket
            /// </summary>
            public ClientWebSocket CurrentWebSocket { get; }
            /// <summary>
            /// 触发事件的群组
            /// </summary>
            public XCCGroup Group { get; }
            internal XCCConnectedEventArgs(XCCGroup group, ClientWebSocket clientWebSocket)
            {
                CurrentWebSocket = clientWebSocket;
                Group = group;
            }
        }
        class XCCGroupImpl : XCCGroup
        {
            internal XCCGroupImpl(string groupId, string sender, XCCNetWorkBase xCCNetWorkBase) : base(groupId, sender, xCCNetWorkBase) { }
        }
        class XCCConnectionClosedEventArgsImpl : XCCConnectionClosedEventArgs
        {
            internal XCCConnectionClosedEventArgsImpl(XCCGroup group, ClientWebSocket clientWebSocket, bool closeNormally) : base(group, clientWebSocket, closeNormally) { }
        }
        class XCCConnectedEventArgsImpl : XCCConnectedEventArgs
        {
            internal XCCConnectedEventArgsImpl(XCCGroup group, ClientWebSocket clientWebSocket) : base(group, clientWebSocket) { }
        }
        class XCCTextMessageReceivedEventArgsImpl : XCCTextMessageReceivedEventArgs
        {
            internal XCCTextMessageReceivedEventArgsImpl(XCCGroup group, ClientWebSocket clientWebSocket, string sender, string messageId, XCCTextMessageType messageType, string message) : base(group, clientWebSocket, sender, messageId, messageType, message) { }
        }
        class XCCBinaryMessageReceivedEventArgsImpl : XCCBinaryMessageReceivedEventArgs
        {
            internal XCCBinaryMessageReceivedEventArgsImpl(XCCGroup group, ClientWebSocket clientWebSocket, string sender, string messageId, byte[] buffer, XCCBinaryMessageType messageType, string signature) : base(group, clientWebSocket, sender, messageId, buffer, messageType, signature) { }
        }
        class XCCExceptionMessageReceivedEventArgsImpl : XCCExceptionMessageReceivedEventArgs
        {
            internal XCCExceptionMessageReceivedEventArgsImpl(XCCGroup group, ClientWebSocket clientWebSocket, string sender, string messageId, XFECyberCommException exception) : base(group, clientWebSocket, sender, messageId, exception) { }
        }
    }
}