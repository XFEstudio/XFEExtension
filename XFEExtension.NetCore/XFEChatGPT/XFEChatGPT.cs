using System.Net.WebSockets;
using System.Text;
using XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;
using XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;
using XFEExtension.NetCore.XFEChatGPT.OtherInnerClass;
using XFEExtension.NetCore.XFETransform;

namespace XFEExtension.NetCore.XFEChatGPT;

/// <summary>
/// 基于gpt.api.xfegzs.com开放接口的ChatGPT问答类
/// </summary>
public class XFEChatGPT : XFEChatGPTBase
{
    #region 私有字段
    private readonly XFEAskGPTMessage _xFEAskGPTMessage;
    #endregion
    #region 公有封装字段
    /// <summary>
    /// ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）
    /// </summary>
    public string? SystemString
    {
        get => _xFEAskGPTMessage.IsSelfEditData ? throw new XFEChatGPTException("自定义数据模式下不可获取") : _xFEAskGPTMessage.SystemContent;
        set
        {
            if (_xFEAskGPTMessage.IsSelfEditData)
            {
                throw new XFEChatGPTException("自定义数据模式下不可设置");
            }

            _xFEAskGPTMessage.SystemContent = value;
        }
    }
    /// <summary>
    /// 向ChatGPT提问的内容
    /// </summary>
    public string? AskContent
    {
        get => _xFEAskGPTMessage.IsSelfEditData ? throw new XFEChatGPTException("自定义数据模式下不可获取") : _xFEAskGPTMessage.AskContent;
        set
        {
            if (_xFEAskGPTMessage.IsSelfEditData)
            {
                throw new XFEChatGPTException("自定义数据模式下不可设置");
            }

            _xFEAskGPTMessage.AskContent = value;
        }
    }
    /// <summary>
    /// ChatGPT的语言模型
    /// </summary>
    public ChatGPTModel GPTModel
    {
        get => _xFEAskGPTMessage.IsSelfEditData ? _xFEAskGPTMessage.EnvironmentGPTData!.Model.GetModel() : _xFEAskGPTMessage.ChatGPTModel!.GetModel();
        set
        {
            if (_xFEAskGPTMessage.IsSelfEditData)
            {
                _xFEAskGPTMessage.EnvironmentGPTData!.Model = value.GetModelString();
            }
            else
            {
                _xFEAskGPTMessage.ChatGPTModel = value.GetModelString();
            }
        }
    }
    /// <summary>
    /// XFE通信协议
    /// </summary>
    public XFEComProtocol XFEComProtocol => _xFEAskGPTMessage.ComProtocol;

    /// <summary>
    /// 设置是否为自定义数据模式
    /// </summary>
    public bool IsSelfEditData => _xFEAskGPTMessage.IsSelfEditData;

    /// <summary>
    /// 流式输出（打字效果）还是一次性输出
    /// </summary>
    public bool StreamMode => _xFEAskGPTMessage.IsSelfEditData ? _xFEAskGPTMessage.EnvironmentGPTData!.Stream : _xFEAskGPTMessage.Stream;

    /// <summary>
    /// ChatGPT的环境数据
    /// </summary>
    public EnvironmentGPTData? EnvironmentGPTData
    {
        get => _xFEAskGPTMessage.EnvironmentGPTData;
        set => _xFEAskGPTMessage.EnvironmentGPTData = value;
    }
    #endregion
    #region 公有方法
    /// <summary>
    /// 向ChatGPT发送消息
    /// </summary>
    /// <param name="messageId">提问的消息ID，用于标识回复</param>
    /// <exception cref="XFEChatGPTException">远程服务器返回出错时抛出</exception>"
    public void SendGPTMessage(string messageId)
    {
        var askGPTThread = new Thread(StartGetGPTMessage);
        askGPTThread.Start(new MessageIdAndThread(messageId, askGPTThread));
        ThreadList.Add(askGPTThread);
    }
    /// <summary>
    /// 向ChatGPT发送指定消息
    /// </summary>
    /// <param name="messageId">提问的消息ID，用于标识回复</param>
    /// <param name="askContent">提问的内容</param>
    /// <exception cref="XFEChatGPTException">远程服务器返回出错时抛出</exception>"
    public void SendGPTMessage(string messageId, string askContent)
    {
        AskContent = askContent;
        SendGPTMessage(messageId);
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    [Obsolete("方法已经过时，请使用MemorableXFEChatGPT")]
    public void Dispose()
    {
        foreach (var thread in ThreadList)
        {
            thread.Abort();
        }
    }
    #endregion
    #region 静态方法
    /// <summary>
    /// 发送消息并获取GPT回复
    /// </summary>
    /// <param name="message">待发送的消息</param>
    /// <returns></returns>
    public static async Task<string> SendAndGetGPTResponse(string message)
    {
        var receivedMessage = string.Empty;
        try
        {
            ClientWebSocket webSocket = new();
            await webSocket.ConnectAsync(new Uri("ws://gpt.api.xfegzs.com/"), CancellationToken.None);
            await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
            var buffer = new byte[1024];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed.", CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return receivedMessage;
    }
    /// <summary>
    /// 获取及更新APIKey秘钥
    /// </summary>
    /// <param name="password">APIKey服务器密码</param>
    /// <param name="apiKeyCommand">APIKey指令</param>
    /// <param name="commandString">待执行的指令</param>
    /// <returns></returns>
    public static async Task<string> GPTAPIKeyCommand(string password, ApiKeyCommand apiKeyCommand, string commandString)
    {
        var receivedMessage = string.Empty;
        try
        {
            var command = apiKeyCommand == ApiKeyCommand.GetApiKey ? "[获取]" : commandString;
            ClientWebSocket webSocket = new();
            webSocket.Options.SetRequestHeader("XFEPassword", password);
            await webSocket.ConnectAsync(new Uri("ws://api.xfegzs.com/"), CancellationToken.None);
            await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(command)), WebSocketMessageType.Text, true, CancellationToken.None);
            var buffer = new byte[1024];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed.", CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return receivedMessage;
    }
    #endregion
    #region 线程方法
    private async void StartGetGPTMessage(object? sender)
    {
        var messageIdAndThread = (MessageIdAndThread)sender!;
        var messageId = messageIdAndThread.MessageId;
        var thread = messageIdAndThread.Thread;
        if (StreamMode)
        {
            var isStarted = false;
            try
            {
                #region 进行HTTP请求
                ClientWebSocket webSocket = new();
                await webSocket.ConnectAsync(new Uri("ws://gpt.api.xfegzs.com/"), CancellationToken.None);
                var json = _xFEAskGPTMessage.ConvertToJson();
                if (json is not null)
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)), WebSocketMessageType.Text, true, CancellationToken.None);
                #endregion
                while (true)
                {
                    #region 读取消息
                    var buffer = new byte[1024];
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);//接收消息
                    var nowReceivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);//将消息转换为字符串
                    #endregion
                    if (XFEComProtocol == XFEComProtocol.XFEFAST)
                    {
                        if (nowReceivedMessage == "[DONE]")
                        {
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFEDONE]", messageId, GenerateState.End));
                            break;
                        }

                        if (nowReceivedMessage.Contains("[XFERemoteAPIError]"))
                        {
                            if (!isStarted)
                            {
                                XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start));
                            }
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error));
                        }
                        else if (nowReceivedMessage == "[XFE]")
                        {
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start));
                        }
                        else
                        {
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Continue));
                        }
                    }
                    else
                    {
                        if (nowReceivedMessage == "[DONE]")
                        {
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFEDONE]", messageId, GenerateState.End));
                            break;
                        }

                        if (nowReceivedMessage.Contains("[XFERemoteAPIError]"))
                        {
                            if (!isStarted)
                            {
                                XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start));
                            }
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error));
                        }
                        else if (nowReceivedMessage == "[XFE]")
                        {
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start));
                        }
                        else
                        {
                            if (nowReceivedMessage.Contains("[XFE]"))
                            {
                                nowReceivedMessage = nowReceivedMessage.Replace("[XFE]", string.Empty);
                                XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Continue));
                            }
                            else
                            {
                                if (!isStarted)
                                {
                                    XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start));
                                }
                                XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error));
                            }
                        }
                    }
                }
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed.", CancellationToken.None);
            }
            catch (Exception ex) when (ex is not XFEChatGPTException)
            {
                if (!isStarted)
                {
                    XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start));
                }
                XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(ex.ToString(), messageId, GenerateState.Error));
                Console.WriteLine(ex.ToString());
            }
        }
        else
        {
            try
            {
                #region 进行HTTP请求
                ClientWebSocket webSocket = new();
                await webSocket.ConnectAsync(new Uri("ws://gpt.api.xfegzs.com/"), CancellationToken.None);
                var json = _xFEAskGPTMessage.ConvertToJson();
                if (json is not null)
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)), WebSocketMessageType.Text, true, CancellationToken.None);
                #endregion
                #region 读取消息
                var buffer = new byte[1024];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);//接收消息
                var nowReceivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);//将消息转换为字符串
                #endregion
                if (XFEComProtocol == XFEComProtocol.XFEFAST)
                {
                    XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Start));
                }
                else
                {
                    if (nowReceivedMessage.Contains("[XFE]"))
                    {
                        nowReceivedMessage = nowReceivedMessage.Replace("[XFE]", string.Empty);
                        XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Start));
                    }
                    else
                    {
                        XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error));
                    }
                }
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed.", CancellationToken.None);
            }
            catch (Exception ex)
            {
                XFEChatGPTMessageReceived?.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(ex.ToString(), messageId, GenerateState.Error));
                Console.WriteLine(ex.ToString());
            }
        }
        ThreadList.Remove(thread);
    }
    #endregion
    #region 公有事件
    /// <summary>
    /// 当收到ChatGPT的消息时触发
    /// </summary>
    public event EventHandler<XFEGPTMessageReceivedEventArgs>? XFEChatGPTMessageReceived;
    #endregion
    #region 构造方法
    /// <summary>
    /// 从EnvironmentGPTData构造，使用默认通信协议（快速响应通讯协议）
    /// </summary>
    /// <param name="environmentGPTData">ChatGPT环境设置</param>
    public XFEChatGPT(EnvironmentGPTData environmentGPTData) => _xFEAskGPTMessage = new XFEAskGPTMessage(true, environmentGPTData.Stream, environmentGPTData.Model, environmentGPTData, XFEComProtocol.XFEFAST, string.Empty, string.Empty);
    /// <summary>
    /// 从EnvironmentGPTData构造，自定义通讯协议
    /// </summary>
    /// <param name="environmentGPTData">ChatGPT环境设置</param>
    /// <param name="comProtocol">通信协议</param>
    public XFEChatGPT(EnvironmentGPTData environmentGPTData, XFEComProtocol comProtocol) => _xFEAskGPTMessage = new XFEAskGPTMessage(true, environmentGPTData.Stream, environmentGPTData.Model, environmentGPTData, comProtocol, string.Empty, string.Empty);
    #region 默认模型构造
    /// <summary>
    /// 使用系统和内容构造，使用默认通信协议（快速响应通讯协议）
    /// </summary>
    /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
    /// <param name="askContent">向ChatGPT提问的内容</param>
    public XFEChatGPT(string systemContent, string askContent)
    {
        _xFEAskGPTMessage = new XFEAskGPTMessage(false, false, ChatGPTModel.gpt3point5turbo.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, askContent);
    }
    /// <summary>
    /// 使用系统和内容构造，可设置流式输出（打字效果）还是一次性输出，使用默认通信协议（快速响应通讯协议）
    /// </summary>
    /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
    /// <param name="askContent">向ChatGPT提问的内容</param>
    /// <param name="stream">设置流式输出（打字效果）还是一次性输出</param>
    public XFEChatGPT(string systemContent, string askContent, bool stream)
    {
        _xFEAskGPTMessage = new XFEAskGPTMessage(false, stream, ChatGPTModel.gpt3point5turbo.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, askContent);
    }
    /// <summary>
    /// 使用系统构造
    /// </summary>
    /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
    public XFEChatGPT(string systemContent)
    {
        _xFEAskGPTMessage = new XFEAskGPTMessage(false, false, ChatGPTModel.gpt3point5turbo.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, string.Empty);
    }
    /// <summary>
    /// 使用系统构造，可设置流式输出（打字效果）还是一次性输出，自定义通讯协议
    /// </summary>
    /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
    /// <param name="stream">设置流式输出（打字效果）还是一次性输出</param>
    public XFEChatGPT(string systemContent, bool stream)
    {
        _xFEAskGPTMessage = new XFEAskGPTMessage(false, stream, ChatGPTModel.gpt3point5turbo.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, string.Empty);
    }
    /// <summary>
    /// 使用系统构造，可设置流式输出（打字效果）还是一次性输出，自定义通讯协议
    /// </summary>
    /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
    /// <param name="stream">设置流式输出（打字效果）还是一次性输出</param>
    /// <param name="comProtocol">通信协议</param>
    public XFEChatGPT(string systemContent, bool stream, XFEComProtocol comProtocol)
    {
        _xFEAskGPTMessage = new XFEAskGPTMessage(false, stream, ChatGPTModel.gpt3point5turbo.GetModelString(), null, comProtocol, systemContent, string.Empty);
    }
    #endregion
    #region 自定义模型构造
    /// <summary>
    /// 使用系统和内容构造，自定义模型，使用默认通信协议（快速响应通讯协议）
    /// </summary>
    /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
    /// <param name="askContent">向ChatGPT提问的内容</param>
    /// <param name="chatGPTModel">选用的GPT模型</param>
    public XFEChatGPT(string systemContent, string askContent, ChatGPTModel chatGPTModel)
    {
        _xFEAskGPTMessage = new XFEAskGPTMessage(false, false, chatGPTModel.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, askContent);
    }
    /// <summary>
    /// 使用系统和内容构造，可设置流式输出（打字效果）还是一次性输出，自定义模型，使用默认通信协议（快速响应通讯协议）
    /// </summary>
    /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
    /// <param name="askContent">向ChatGPT提问的内容</param>
    /// <param name="stream">设置流式输出（打字效果）还是一次性输出</param>
    /// <param name="chatGPTModel">选用的GPT模型</param>
    public XFEChatGPT(string systemContent, string askContent, bool stream, ChatGPTModel chatGPTModel)
    {
        _xFEAskGPTMessage = new XFEAskGPTMessage(false, stream, chatGPTModel.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, askContent);
    }
    /// <summary>
    /// 使用系统构造，自定义模型，使用默认通信协议（快速响应通讯协议）
    /// </summary>
    /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
    /// <param name="chatGPTModel">选用的GPT模型</param>
    public XFEChatGPT(string systemContent, ChatGPTModel chatGPTModel)
    {
        _xFEAskGPTMessage = new XFEAskGPTMessage(false, false, chatGPTModel.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, string.Empty);
    }
    /// <summary>
    /// 使用系统构造，可设置流式输出（打字效果）还是一次性输出，自定义模型，使用默认通信协议（快速响应通讯协议）
    /// </summary>
    /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
    /// <param name="stream">设置流式输出（打字效果）还是一次性输出</param>
    /// <param name="chatGPTModel">选用的GPT模型</param>
    public XFEChatGPT(string systemContent, bool stream, ChatGPTModel chatGPTModel)
    {
        _xFEAskGPTMessage = new XFEAskGPTMessage(false, stream, chatGPTModel.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, string.Empty);
    }
    /// <summary>
    /// 使用系统构造，可设置流式输出（打字效果）还是一次性输出，自定义模型，自定义通讯协议
    /// </summary>
    /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
    /// <param name="stream">设置流式输出（打字效果）还是一次性输出</param>
    /// <param name="chatGPTModel">选用的GPT模型</param>
    /// <param name="comProtocol">通信协议</param>
    public XFEChatGPT(string systemContent, bool stream, ChatGPTModel chatGPTModel, XFEComProtocol comProtocol)
    {
        _xFEAskGPTMessage = new XFEAskGPTMessage(false, stream, chatGPTModel.GetModelString(), null, comProtocol, systemContent, string.Empty);
    }
    #endregion
    #endregion
}