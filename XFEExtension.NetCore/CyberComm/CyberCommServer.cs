﻿using System.Collections.Specialized;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace XFEExtension.NetCore.CyberComm;

/// <summary>
/// CyberComm服务器
/// </summary>
public class CyberCommServer
{
    private readonly string[] serverURLs;
    #region 公共属性
    /// <summary>
    /// 字节流缓冲区长度
    /// </summary>
    public int BufferLength { get; set; } = 1024;
    /// <summary>
    /// 服务器正在运行
    /// </summary>
    public bool ServerRunning { get; set; }
    /// <summary>
    /// 是否自动接收完整消息
    /// </summary>
    public bool AutoReceiveCompletedMessage { get; set; }
    /// <summary>
    /// 收到消息时触发
    /// </summary>
    public event EventHandler<CyberCommServerEventArgs>? MessageReceived;
    /// <summary>
    /// 客户端连接时触发
    /// </summary>
    public event EventHandler<CyberCommServerEventArgs>? ClientConnected;
    /// <summary>
    /// 服务器启动时触发
    /// </summary>
    public event EventHandler? ServerStarted;
    /// <summary>
    /// 连接关闭时触发
    /// </summary>
    public event EventHandler<CyberCommServerEventArgs>? ConnectionClosed;
    /// <summary>
    /// Http请求接收时触发
    /// </summary>
    public event EventHandler<CyberCommRequestEventArgs>? RequestReceived;
    /// <summary>
    /// 服务器端
    /// </summary>
    public HttpListener Server { get; } = new();
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
            foreach (var url in serverURLs)
            {
                Server.Prefixes.Add(url);
            }
            Server.Start();
            ServerRunning = true;
            ServerStarted?.Invoke(this, EventArgs.Empty);
            while (ServerRunning)
            {
                var httpListenerContext = await Server.GetContextAsync();
                var requestURL = httpListenerContext.Request.Url;
                var clientIP = httpListenerContext.Request.RemoteEndPoint.Address.ToString();
                if (httpListenerContext.Request.IsWebSocketRequest)
                {
                    var httpListenerWebSocketContext = await httpListenerContext.AcceptWebSocketAsync(null);
                    var webSocket = httpListenerWebSocketContext.WebSocket;
                    var wsHeader = httpListenerWebSocketContext.Headers;
                    ClientConnected?.Invoke(this, new CyberCommServerEventArgsImpl(requestURL, webSocket, string.Empty, clientIP, wsHeader, true));
                    CyberCommClientConnected(requestURL, webSocket, wsHeader, clientIP);
                }
                else
                {
                    var request = httpListenerContext.Request;
                    var response = httpListenerContext.Response;
                    var requestMethod = request.HttpMethod;
                    var headers = request.Headers;
                    var queryString = request.QueryString;
                    if (requestMethod == "POST" && AutoReceiveCompletedMessage)
                    {
                        _ = Task.Run(() =>
                        {
                            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                            string postData = reader.ReadToEnd();
                            RequestReceived?.Invoke(this, new CyberCommRequestEventArgsImpl(requestURL, requestMethod, postData, headers, queryString, request, response, clientIP));
                        });
                    }
                    else
                    {
                        RequestReceived?.Invoke(this, new CyberCommRequestEventArgsImpl(requestURL, requestMethod, null, headers, queryString, request, response, clientIP));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new XFECyberCommException("启动服务器时发生异常", ex);
        }
    }
    /// <summary>
    /// 停止CyberComm服务器
    /// </summary>
    public void StopCyberCommServer()
    {
        ServerRunning = false;
        Server.Close();
    }
    private async void CyberCommClientConnected(Uri? requestURL, WebSocket webSocket, NameValueCollection wsHeader, string clientIP)
    {
        while (webSocket.State == WebSocketState.Open)
        {
            try
            {
                byte[] receiveBuffer = new byte[BufferLength];
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
                    MessageReceived?.Invoke(this, new CyberCommServerEventArgsImpl(requestURL, webSocket, receivedMessage, clientIP, wsHeader, receiveResult.EndOfMessage));
                }
                if (receiveResult.MessageType == WebSocketMessageType.Binary)
                {
                    MessageReceived?.Invoke(this, new CyberCommServerEventArgsImpl(requestURL, webSocket, receivedBinaryBuffer, clientIP, wsHeader, receiveResult.EndOfMessage));
                }
                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection Closed", CancellationToken.None);
                    break;
                }
            }
            catch (Exception ex)
            {
                MessageReceived?.Invoke(this, new CyberCommServerEventArgsImpl(requestURL, webSocket, new XFECyberCommException("与客户端端通讯期间发生异常", ex), clientIP, wsHeader));
                break;
            }
        }
        ConnectionClosed?.Invoke(this, new CyberCommServerEventArgsImpl(requestURL, webSocket, string.Empty, clientIP, wsHeader, true));
        webSocket.Dispose();
    }
    #endregion
    #region 构造函数
    /// <summary>
    /// CyberComm服务器，使用端口创建
    /// </summary>
    /// <param name="listenPorts">监听端口</param>
    public CyberCommServer(params int[] listenPorts)
    {
        var serverURLs = new List<string>();
        foreach (var port in listenPorts)
        {
            serverURLs.Add($"http://*:{port}/");
        }
        this.serverURLs = [.. serverURLs];
        AutoReceiveCompletedMessage = true;
    }
    /// <summary>
    /// CyberComm服务器，使用URL创建
    /// </summary>
    /// <param name="serverURLs">服务器URL</param>
    public CyberCommServer(params string[] serverURLs)
    {
        this.serverURLs = serverURLs;
        AutoReceiveCompletedMessage = true;
    }
    /// <summary>
    /// CyberComm服务器，使用URL创建
    /// </summary>
    /// <param name="autoReceiveCompletedMessage">是否自动接收完整消息</param>
    /// <param name="serverURLs">服务器URL</param>
    public CyberCommServer(bool autoReceiveCompletedMessage = true, params string[] serverURLs)
    {
        this.serverURLs = serverURLs;
        AutoReceiveCompletedMessage = autoReceiveCompletedMessage;
    }
    #endregion
}