using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public async Task StartCyberCommClient()
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
        public async Task SendTextMessage(string message)
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
        public async Task SendBinaryMessage(byte[] message)
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
        public async Task CloseCyberCommClient()
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
        public async Task StartCyberCommServer()
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
        /// <param name="listenPort">监听端口</param>
        /// <param name="autoReceiveCompletedMessage">是否自动接收完整消息</param>
        public CyberCommServer(int listenPort, bool autoReceiveCompletedMessage = true)
        {
            this.serverURL = $"http://*:{listenPort}/";
            this.AutoReceiveCompletedMessage = autoReceiveCompletedMessage;
        }
        /// <summary>
        /// CyberComm服务器，使用URL创建
        /// </summary>
        /// <param name="serverURL">服务器URL</param>
        /// <param name="autoReceiveCompletedMessage">是否自动接收完整消息</param>
        public CyberCommServer(string serverURL, bool autoReceiveCompletedMessage = true)
        {
            this.serverURL = serverURL;
            this.AutoReceiveCompletedMessage = autoReceiveCompletedMessage;
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
        public async Task ReplyMessage(string message)
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
        public async Task Close()
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
        public async Task ReplyMessage(string message)
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
        public async Task ReplyBinaryMessage(byte[] bytes)
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
        public async Task Close()
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
        private readonly List<CyberCommServerEventArgs> cyberCommList = new List<CyberCommServerEventArgs>();
        /// <summary>
        /// 组ID
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// 添加客户端
        /// </summary>
        /// <param name="e">客户端</param>
        public void Add(CyberCommServerEventArgs e)
        {
            cyberCommList.Add(e);
        }
        /// <summary>
        /// 移除指定的客户端
        /// </summary>
        /// <param name="webSocket">客户端</param>
        public void Remove(WebSocket webSocket)
        {
            cyberCommList.Remove(cyberCommList.Find(x => x.CurrentWebSocket == webSocket));
        }
        /// <summary>
        /// 移除指定索引的客户端
        /// </summary>
        /// <param name="index">客户单索引</param>
        public void RemoveAt(int index)
        {
            cyberCommList.RemoveAt(index);
        }
        /// <summary>
        /// 清空列表
        /// </summary>
        public void Clear()
        {
            cyberCommList.Clear();
        }
        /// <summary>
        /// 群组客户端数量
        /// </summary>
        public int Count
        {
            get
            {
                return cyberCommList.Count;
            }
        }
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="findFunc">查找</param>
        /// <returns>客户端</returns>
        public CyberCommServerEventArgs this[Predicate<CyberCommServerEventArgs> findFunc]
        {
            get
            {
                return cyberCommList.Find(findFunc);
            }
        }
        /// <summary>
        /// 发送群组文本消息
        /// </summary>
        /// <param name="message">群发文本消息</param>
        public async Task SendGroupTextMessage(string message)
        {
            List<Task> tasks = new List<Task>();
            foreach (CyberCommServerEventArgs cyberCommServerEventArgs in cyberCommList)
            {
                tasks.Add(cyberCommServerEventArgs.ReplyMessage(message));
            }
            await Task.WhenAll(tasks);
        }
        /// <summary>
        /// 向指定的WS客户端发送群组文本消息
        /// </summary>
        /// <param name="message">群发文本消息</param>
        /// <param name="findFunc">指定的WS客户端</param>
        public async Task SendGroupTextMessage(string message, Func<CyberCommServerEventArgs, bool> findFunc)
        {
            List<Task> tasks = new List<Task>();
            foreach (CyberCommServerEventArgs cyberCommServerEventArgs in cyberCommList)
            {
                if (findFunc.Invoke(cyberCommServerEventArgs))
                {
                    tasks.Add(cyberCommServerEventArgs.ReplyMessage(message));
                }
            }
            await Task.WhenAll(tasks);
        }
        /// <summary>
        /// 发送群组二进制消息
        /// </summary>
        /// <param name="bytes">群发二进制消息</param>
        public async Task SendGroupBinaryMessage(byte[] bytes)
        {
            List<Task> tasks = new List<Task>();
            foreach (CyberCommServerEventArgs cyberCommServerEventArgs in cyberCommList)
            {
                tasks.Add(cyberCommServerEventArgs.ReplyBinaryMessage(bytes));
            }
            await Task.WhenAll(tasks);
        }
        /// <summary>
        /// 向指定的WS客户端发送群组二进制消息
        /// </summary>
        /// <param name="bytes">群发二进制消息</param>
        /// <param name="findFunc">指定的WS客户端</param>
        public async Task SendGroupBinaryMessage(byte[] bytes, Func<CyberCommServerEventArgs, bool> findFunc)
        {
            List<Task> tasks = new List<Task>();
            foreach (CyberCommServerEventArgs cyberCommServerEventArgs in cyberCommList)
            {
                if (findFunc.Invoke(cyberCommServerEventArgs))
                {
                    tasks.Add(cyberCommServerEventArgs.ReplyBinaryMessage(bytes));
                }
            }
            await Task.WhenAll(tasks);
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
        /// <param name="cyberCommList">客户端群组</param>
        public CyberCommGroup(string GroupId, List<CyberCommServerEventArgs> cyberCommList)
        {
            this.GroupId = GroupId;
            this.cyberCommList = cyberCommList;
        }
    }
    /// <summary>
    /// 代签名的WebSocket
    /// </summary>
    public class SignedWebSocket
    {
        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }
        /// <summary>
        /// 服务器
        /// </summary>
        public WebSocket WebSocket { get; set; }
        /// <summary>
        /// 签名WebSocket
        /// </summary>
        /// <param name="signature">签名</param>
        /// <param name="webSocket">WebSocket</param>
        public SignedWebSocket(string signature, WebSocket webSocket)
        {
            Signature = signature;
            WebSocket = webSocket;
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
        public async Task SendGroupTextMessage(string GroupId, string message)
        {
            CyberCommGroup commGroup = this[GroupId];
            if (commGroup != null)
            {
                await commGroup.SendGroupTextMessage(message);
            }
        }
        /// <summary>
        /// 通信群组控制器发送二进制消息
        /// </summary>
        /// <param name="GroupId">目标群组的ID</param>
        /// <param name="bytes">发送的二进制消息</param>
        public async Task SendGroupBinaryMessage(string GroupId, byte[] bytes)
        {
            CyberCommGroup commGroup = this[GroupId];
            if (commGroup != null)
            {
                await commGroup.SendGroupBinaryMessage(bytes);
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
}