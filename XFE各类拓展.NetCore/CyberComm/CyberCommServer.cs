using System.Collections.Specialized;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace XFE各类拓展.NetCore.CyberComm
{
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
            serverURL = $"http://*:{listenPort}/";
            AutoReceiveCompletedMessage = autoReceiveCompletedMessage;
        }
        /// <summary>
        /// CyberComm服务器，使用URL创建
        /// </summary>
        /// <param name="serverURL">服务器URL</param>
        /// <param name="autoReceiveCompletedMessage">是否自动接收完整消息</param>
        public CyberCommServer(string serverURL, bool autoReceiveCompletedMessage = true)
        {
            this.serverURL = serverURL;
            AutoReceiveCompletedMessage = autoReceiveCompletedMessage;
        }
        #endregion
    }
}