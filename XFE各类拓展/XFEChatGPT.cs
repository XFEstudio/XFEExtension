using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XFE各类拓展.XFEJsonTransform;

namespace XFE各类拓展.XFEChatGPT
{
    /// <summary>
    /// APIKey命令
    /// </summary>
    public enum ApiKeyCommand
    {
        /// <summary>
        /// 获取APIKey列表
        /// </summary>
        GetApiKey,
        /// <summary>
        /// 设置APIKey列表
        /// </summary>
        SetApiKey
    }
    /// <summary>
    /// XFE通讯协议
    /// </summary>
    public enum XFEComProtocol
    {
        /// <summary>
        /// 响应速度快速，不安全的通讯协议
        /// </summary>
        XFEFAST,
        /// <summary>
        /// 响应速度适中，安全的通讯协议
        /// </summary>
        XFEHARD
    }
    /// <summary>
    /// 文本生成状态
    /// </summary>
    public enum GenerateState
    {
        /// <summary>
        /// 表示生成已经开始
        /// </summary>
        Start,
        /// <summary>
        /// 表示生成正在进行中
        /// </summary>
        Continue,
        /// <summary>
        /// 表示生成已经结束
        /// </summary>
        End,
        /// <summary>
        /// 表示生成出错
        /// </summary>
        Error
    }
    /// <summary>
    /// ChatGPT的模型
    /// </summary>
    public enum ChatGPTModel
    {
        /// <summary>
        /// 比任何 GPT-3.5 型号都更强大，能够执行更复杂的任务，并针对聊天进行了优化。将在发布 2 周后使用我们最新的模型迭代进行更新。
        /// </summary>
        gpt4,
        /// <summary>
        /// 最新的GPT-4模型，具有改进的指令跟随、JSON模式、可复制输出、并行函数调用等功能。最多返回4096个输出标记。此预览模型还不适合生产流量。
        /// </summary>
        gpt4turbo,
        /// <summary>
        /// 能够理解图像，以及所有其他GPT-4 Turbo功能。最多返回4096个输出标记。这是一个预览模型版本，还不适合生产流量。
        /// </summary>
        gpt4turbovision,
        /// <summary>
        /// 13 年 2023 月 3 日带有函数调用数据的快照。与gpt-4不同，此模型不会收到更新，并将在新版本发布3个月后弃用。基于gpt-4
        /// </summary>
        gpt40613,
        /// <summary>
        /// 功能与gpt4基本模型相同，但上下文长度是其 4 倍。将使用最新的模型迭代进行更新。基于gpt-4
        /// </summary>
        gpt432k,
        /// <summary>
        /// 13 年 2023 月 3 日的快照。与GPT-4-32K不同，此模型不会收到更新，并将在新版本发布 3 个月后弃用。基于gpt-4-32k
        /// </summary>
        gpt432k0613,
        /// <summary>
        /// 功能最强大的 GPT-3.5 型号，针对聊天进行了优化，成本仅为 .将在发布 1 周后使用最新的模型迭代进行更新。
        /// </summary>
        gpt3point5turbo,
        /// <summary>
        /// 功能与标准模型相同，但上下文是其 4 倍。
        /// </summary>
        gpt3point5turbo16k,
        /// <summary>
        /// 13 年 2023 月 3 日的快照。与GPT-3.5-16K不同，此模型不会收到更新，并将在新版本发布 3 个月后弃用。
        /// </summary>
        gpt3point5turbo0613,
        /// <summary>
        /// 13 年 2023 月 3 日的快照。与GPT-3.5-turbo-16k不同，此模型不会收到更新，并将在新版本发布 3 个月后弃用。
        /// </summary>
        gpt3point5turbo16k0613,
        /// <summary>
        /// 可以完成任何语言任务，质量更好，输出时间更长，并且遵循一致的指令，而不是居里，巴贝奇或ADA模型。还支持一些附加功能，例如插入文本。
        /// </summary>
        textdavinci003,
        /// <summary>
        /// 与监督微调而不是强化学习类似的能力，但经过训练。
        /// </summary>
        textdavinci002,
        /// <summary>
        /// 针对代码完成任务进行优化的模型
        /// </summary>
        codedavinci002
    }
    /// <summary>
    /// ChatGPT模型的拓展
    /// </summary>
    public static class ChatGPTModelExtension
    {
        /// <summary>
        /// 获取模型的字符串
        /// </summary>
        /// <param name="chatGPTModel"></param>
        /// <returns></returns>
        /// <exception cref="XFEChatGPTException">意料之外的模型</exception>
        public static string GetModelString(this ChatGPTModel chatGPTModel)
        {
            switch (chatGPTModel)
            {
                case ChatGPTModel.gpt4:
                    return "gpt-4";
                case ChatGPTModel.gpt4turbo:
                    return "gpt-4-1106-preview";
                case ChatGPTModel.gpt4turbovision:
                    return "gpt-4-vision-preview";
                case ChatGPTModel.gpt40613:
                    return "gpt-4-0613";
                case ChatGPTModel.gpt432k:
                    return "gpt-4-32k";
                case ChatGPTModel.gpt432k0613:
                    return "gpt-4-32k-0613";
                case ChatGPTModel.gpt3point5turbo:
                    return "gpt-3.5-turbo";
                case ChatGPTModel.gpt3point5turbo16k:
                    return "gpt-3.5-turbo-16k";
                case ChatGPTModel.gpt3point5turbo0613:
                    return "gpt-3.5-turbo-0613";
                case ChatGPTModel.gpt3point5turbo16k0613:
                    return "gpt-3.5-turbo-16k-0613";
                case ChatGPTModel.textdavinci003:
                    return "text-davinci-003";
                case ChatGPTModel.textdavinci002:
                    return "text-davinci-002";
                case ChatGPTModel.codedavinci002:
                    return "code-davinci-002";
                default:
                    throw new XFEChatGPTException("未知的ChatGPT模型");
            }
        }
        /// <summary>
        /// 通过字符串获取模型
        /// </summary>
        /// <param name="chatGPTModel"></param>
        /// <returns></returns>
        /// <exception cref="XFEChatGPTException">意料之外的模型</exception>
        public static ChatGPTModel GetModel(this string chatGPTModel)
        {
            switch (chatGPTModel)
            {
                case "gpt-3.5-turbo":
                    return ChatGPTModel.gpt3point5turbo;
                case "gpt-3.5-turbo-16k":
                    return ChatGPTModel.gpt3point5turbo16k;
                case "gpt-3.5-turbo-0613":
                    return ChatGPTModel.gpt3point5turbo0613;
                case "gpt-3.5-turbo-16k-0613":
                    return ChatGPTModel.gpt3point5turbo16k0613;
                case "text-davinci-003":
                    return ChatGPTModel.textdavinci003;
                case "text-davinci-002":
                    return ChatGPTModel.textdavinci002;
                case "code-davinci-002":
                    return ChatGPTModel.codedavinci002;
                default:
                    throw new XFEChatGPTException("未知的ChatGPT模型");
            }
        }
    }
    /// <summary>
    /// XFEChatGPT的基类
    /// </summary>
    public class XFEChatGPTBase
    {
        private protected List<Thread> threadList = new List<Thread>();
    }
    /// <summary>
    /// 基于gpt.api.xfegzs.com开放接口的ChatGPT问答类
    /// </summary>
    public class XFEChatGPT : XFEChatGPTBase
    {
        #region 私有字段
        private readonly XFEAskGPTMessage xFEAskGPTMessage;
        #endregion
        #region 公有封装字段
        /// <summary>
        /// ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）
        /// </summary>
        public string SystemString
        {
            get
            {
                if (xFEAskGPTMessage.isSelfEditData)
                {
                    throw new XFEChatGPTException("自定义数据模式下不可获取");
                }
                else
                {
                    return xFEAskGPTMessage.systemContent;
                }
            }
            set
            {
                if (xFEAskGPTMessage.isSelfEditData)
                {
                    throw new XFEChatGPTException("自定义数据模式下不可设置");
                }
                else
                {
                    xFEAskGPTMessage.systemContent = value;
                }
            }
        }
        /// <summary>
        /// 向ChatGPT提问的内容
        /// </summary>
        public string AskContent
        {
            get
            {
                if (xFEAskGPTMessage.isSelfEditData)
                {
                    throw new XFEChatGPTException("自定义数据模式下不可获取");
                }
                else
                {
                    return xFEAskGPTMessage.askContent;
                }
            }
            set
            {
                if (xFEAskGPTMessage.isSelfEditData)
                {
                    throw new XFEChatGPTException("自定义数据模式下不可设置");
                }
                else
                {
                    xFEAskGPTMessage.askContent = value;
                }
            }
        }
        /// <summary>
        /// ChatGPT的语言模型
        /// </summary>
        public ChatGPTModel GPTModel
        {
            get
            {
                if (xFEAskGPTMessage.isSelfEditData)
                {
                    return xFEAskGPTMessage.EnvironmentGPTData.model.GetModel();
                }
                else
                {
                    return xFEAskGPTMessage.chatGPTModel.GetModel();
                }
            }
            set
            {
                if (xFEAskGPTMessage.isSelfEditData)
                {
                    xFEAskGPTMessage.EnvironmentGPTData.model = value.GetModelString();
                }
                else
                {
                    xFEAskGPTMessage.chatGPTModel = value.GetModelString();
                }
            }
        }
        /// <summary>
        /// XFE通信协议
        /// </summary>
        public XFEComProtocol XFEComProtocol
        {
            get
            {
                return xFEAskGPTMessage.comProtocol;
            }
        }
        /// <summary>
        /// 设置是否为自定义数据模式
        /// </summary>
        public bool IsSelfEditData
        {
            get
            {
                return xFEAskGPTMessage.isSelfEditData;
            }
        }
        /// <summary>
        /// 流式输出（打字效果）还是一次性输出
        /// </summary>
        public bool StreamMode
        {
            get
            {
                if (xFEAskGPTMessage.isSelfEditData)
                {
                    return xFEAskGPTMessage.EnvironmentGPTData.stream;
                }
                else
                {
                    return xFEAskGPTMessage.stream;
                }
            }
        }
        /// <summary>
        /// ChatGPT的环境数据
        /// </summary>
        public EnvironmentGPTData EnvironmentGPTData
        {
            get
            {
                return xFEAskGPTMessage.EnvironmentGPTData;
            }
            set
            {
                xFEAskGPTMessage.EnvironmentGPTData = value;
            }
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
            threadList.Add(askGPTThread);
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
        public void Dispose()
        {
            foreach (var thread in threadList)
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
            string receivedMessage = string.Empty;
            try
            {
                ClientWebSocket webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri("ws://gpt.api.xfegzs.com/"), CancellationToken.None);
                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
                byte[] buffer = new byte[1024];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
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
            string receivedMessage = string.Empty;
            string command = string.Empty;
            try
            {
                if (apiKeyCommand == ApiKeyCommand.GetApiKey)
                {
                    command = "[获取]";
                }
                else
                {
                    command = commandString;
                }
                ClientWebSocket webSocket = new ClientWebSocket();
                webSocket.Options.SetRequestHeader("XFEPassword", password);
                await webSocket.ConnectAsync(new Uri("ws://api.xfegzs.com/"), CancellationToken.None);
                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(command)), WebSocketMessageType.Text, true, CancellationToken.None);
                byte[] buffer = new byte[1024];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
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
        private async void StartGetGPTMessage(object sender)
        {
            var messageIdAndThread = (MessageIdAndThread)sender;
            string messageId = messageIdAndThread.messageId;
            Thread thread = messageIdAndThread.thread;
            if (StreamMode)
            {
                bool isStarted = false;
                try
                {
                    #region 进行HTTP请求
                    ClientWebSocket webSocket = new ClientWebSocket();
                    await webSocket.ConnectAsync(new Uri("ws://gpt.api.xfegzs.com/"), CancellationToken.None);
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(xFEAskGPTMessage.ConvertToJson())), WebSocketMessageType.Text, true, CancellationToken.None);
                    #endregion
                    while (true)
                    {
                        #region 读取消息
                        byte[] buffer = new byte[1024];
                        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);//接收消息
                        var nowReceivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);//将消息转换为字符串
                        #endregion
                        if (XFEComProtocol == XFEComProtocol.XFEFAST)
                        {
                            if (nowReceivedMessage == "[DONE]")
                            {
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFEDONE]", messageId, GenerateState.End));
                                break;
                            }
                            else if (nowReceivedMessage.Contains("[XFERemoteAPIError]"))
                            {
                                if (!isStarted)
                                {
                                    XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start));
                                }
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error));
                            }
                            else if (nowReceivedMessage == "[XFE]")
                            {
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start));
                            }
                            else
                            {
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Continue));
                            }
                        }
                        else
                        {
                            if (nowReceivedMessage == "[DONE]")
                            {
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFEDONE]", messageId, GenerateState.End));
                                break;
                            }
                            else if (nowReceivedMessage.Contains("[XFERemoteAPIError]"))
                            {
                                if (!isStarted)
                                {
                                    XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start));
                                }
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error));
                            }
                            else if (nowReceivedMessage == "[XFE]")
                            {
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start));
                            }
                            else
                            {
                                if (nowReceivedMessage.Contains("[XFE]"))
                                {
                                    nowReceivedMessage = nowReceivedMessage.Replace("[XFE]", string.Empty);
                                    XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Continue));
                                }
                                else
                                {
                                    if (!isStarted)
                                    {
                                        XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start));
                                    }
                                    XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error));
                                }
                            }
                        }
                    }
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed.", CancellationToken.None);
                }
                catch (Exception ex) when (!(ex is XFEChatGPTException))
                {
                    if (!isStarted)
                    {
                        XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start));
                    }
                    XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(ex.ToString(), messageId, GenerateState.Error));
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                try
                {
                    #region 进行HTTP请求
                    ClientWebSocket webSocket = new ClientWebSocket();
                    await webSocket.ConnectAsync(new Uri("ws://gpt.api.xfegzs.com/"), CancellationToken.None);
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(xFEAskGPTMessage.ConvertToJson())), WebSocketMessageType.Text, true, CancellationToken.None);
                    #endregion
                    #region 读取消息
                    byte[] buffer = new byte[1024];
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);//接收消息
                    string nowReceivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);//将消息转换为字符串
                    #endregion
                    if (XFEComProtocol == XFEComProtocol.XFEFAST)
                    {
                        XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Start));
                    }
                    else
                    {
                        if (nowReceivedMessage.Contains("[XFE]"))
                        {
                            nowReceivedMessage = nowReceivedMessage.Replace("[XFE]", string.Empty);
                            XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Start));
                        }
                        else
                        {
                            XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error));
                        }
                    }
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed.", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    XFEChatGPTMessageReceived.Invoke(this, new PrivateXFEGPTMessageReceivedEventArgs(ex.ToString(), messageId, GenerateState.Error));
                    Console.WriteLine(ex.ToString());
                }
            }
            threadList.Remove(thread);
        }
        #endregion
        #region 公有事件
        /// <summary>
        /// 当收到ChatGPT的消息时触发
        /// </summary>
        public event EventHandler<XFEGPTMessageReceivedEventArgs> XFEChatGPTMessageReceived;
        #endregion
        #region 构造方法
        /// <summary>
        /// 从EnvironmentGPTData构造，使用默认通信协议（快速响应通讯协议）
        /// </summary>
        /// <param name="EnvironmentGPTData">ChatGPT环境设置</param>
        public XFEChatGPT(EnvironmentGPTData EnvironmentGPTData)
        {
            xFEAskGPTMessage = new XFEAskGPTMessage(true, EnvironmentGPTData.stream, EnvironmentGPTData.model, EnvironmentGPTData, XFEComProtocol.XFEFAST, string.Empty, string.Empty);
        }
        /// <summary>
        /// 从EnvironmentGPTData构造，自定义通讯协议
        /// </summary>
        /// <param name="EnvironmentGPTData">ChatGPT环境设置</param>
        /// <param name="comProtocol">通信协议</param>
        public XFEChatGPT(EnvironmentGPTData EnvironmentGPTData, XFEComProtocol comProtocol)
        {
            xFEAskGPTMessage = new XFEAskGPTMessage(true, EnvironmentGPTData.stream, EnvironmentGPTData.model, EnvironmentGPTData, comProtocol, string.Empty, string.Empty);
        }
        #region 默认模型构造
        /// <summary>
        /// 使用系统和内容构造，使用默认通信协议（快速响应通讯协议）
        /// </summary>
        /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
        /// <param name="askContent">向ChatGPT提问的内容</param>
        public XFEChatGPT(string systemContent, string askContent)
        {
            xFEAskGPTMessage = new XFEAskGPTMessage(false, false, ChatGPTModel.gpt3point5turbo.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, askContent);
        }
        /// <summary>
        /// 使用系统和内容构造，可设置流式输出（打字效果）还是一次性输出，使用默认通信协议（快速响应通讯协议）
        /// </summary>
        /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
        /// <param name="askContent">向ChatGPT提问的内容</param>
        /// <param name="stream">设置流式输出（打字效果）还是一次性输出</param>
        public XFEChatGPT(string systemContent, string askContent, bool stream)
        {
            xFEAskGPTMessage = new XFEAskGPTMessage(false, stream, ChatGPTModel.gpt3point5turbo.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, askContent);
        }
        /// <summary>
        /// 使用系统构造
        /// </summary>
        /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
        public XFEChatGPT(string systemContent)
        {
            xFEAskGPTMessage = new XFEAskGPTMessage(false, false, ChatGPTModel.gpt3point5turbo.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, string.Empty);
        }
        /// <summary>
        /// 使用系统构造，可设置流式输出（打字效果）还是一次性输出，自定义通讯协议
        /// </summary>
        /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
        /// <param name="stream">设置流式输出（打字效果）还是一次性输出</param>
        public XFEChatGPT(string systemContent, bool stream)
        {
            xFEAskGPTMessage = new XFEAskGPTMessage(false, stream, ChatGPTModel.gpt3point5turbo.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, string.Empty);
        }
        /// <summary>
        /// 使用系统构造，可设置流式输出（打字效果）还是一次性输出，自定义通讯协议
        /// </summary>
        /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
        /// <param name="stream">设置流式输出（打字效果）还是一次性输出</param>
        /// <param name="comProtocol">通信协议</param>
        public XFEChatGPT(string systemContent, bool stream, XFEComProtocol comProtocol)
        {
            xFEAskGPTMessage = new XFEAskGPTMessage(false, stream, ChatGPTModel.gpt3point5turbo.GetModelString(), null, comProtocol, systemContent, string.Empty);
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
            xFEAskGPTMessage = new XFEAskGPTMessage(false, false, chatGPTModel.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, askContent);
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
            xFEAskGPTMessage = new XFEAskGPTMessage(false, stream, chatGPTModel.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, askContent);
        }
        /// <summary>
        /// 使用系统构造，自定义模型，使用默认通信协议（快速响应通讯协议）
        /// </summary>
        /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
        /// <param name="chatGPTModel">选用的GPT模型</param>
        public XFEChatGPT(string systemContent, ChatGPTModel chatGPTModel)
        {
            xFEAskGPTMessage = new XFEAskGPTMessage(false, false, chatGPTModel.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, string.Empty);
        }
        /// <summary>
        /// 使用系统构造，可设置流式输出（打字效果）还是一次性输出，自定义模型，使用默认通信协议（快速响应通讯协议）
        /// </summary>
        /// <param name="systemContent">ChatGPT的系统设置，即设置ChatGPT的背景故事（类比）</param>
        /// <param name="stream">设置流式输出（打字效果）还是一次性输出</param>
        /// <param name="chatGPTModel">选用的GPT模型</param>
        public XFEChatGPT(string systemContent, bool stream, ChatGPTModel chatGPTModel)
        {
            xFEAskGPTMessage = new XFEAskGPTMessage(false, stream, chatGPTModel.GetModelString(), null, XFEComProtocol.XFEFAST, systemContent, string.Empty);
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
            xFEAskGPTMessage = new XFEAskGPTMessage(false, stream, chatGPTModel.GetModelString(), null, comProtocol, systemContent, string.Empty);
        }
        #endregion
        #endregion
    }
    /// <summary>
    /// 基于gpt.api.xfegzs.com开放接口的ChatGPT问答类（记忆模式）
    /// </summary>
    public class MemorableXFEChatGPT : XFEChatGPTBase
    {
        private readonly XFEGPTMemoryDialog xFEGPTMemoryDialog;
        /// <summary>
        /// 获取或设置对话记录
        /// </summary>
        public XFEGPTMemoryDialog MemoryDialog
        {
            get
            {
                return xFEGPTMemoryDialog;
            }
        }
        #region 公共方法
        #region 创建对话
        /// <summary>
        /// 创建一个新的对话，使用默认通信协议（快速响应通讯协议）
        /// </summary>
        /// <param name="dialogId">对话的ID</param>
        /// <param name="system">系统消息</param>
        public void CreateDialog(string dialogId, string system)
        {
            xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection(new XFEGPTMessage[] { new XFEGPTMessage(dialogId, new GPTMessage("system", system)) }, false, ChatGPTModel.gpt3point5turbo, XFEComProtocol.XFEFAST, false, 0.7));
        }
        /// <summary>
        /// 创建一个新的对话，使用默认通信协议（快速响应通讯协议）
        /// </summary>
        /// <param name="dialogId">对话的ID</param>
        /// <param name="system">系统消息</param>
        /// <param name="hasMemory">是否记录对话信息</param>
        public void CreateDialog(string dialogId, string system, bool hasMemory)
        {
            xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection(new XFEGPTMessage[] { new XFEGPTMessage(dialogId, new GPTMessage("system", system)) }, false, ChatGPTModel.gpt3point5turbo, XFEComProtocol.XFEFAST, hasMemory, 0.7));
        }
        /// <summary>
        /// 创建一个新的对话，使用默认通信协议（快速响应通讯协议）
        /// </summary>
        /// <param name="dialogId">对话的ID</param>
        /// <param name="system">系统消息</param>
        /// <param name="hasMemory">是否记录对话信息</param>
        /// <param name="streamMode">是否为流式输出</param>
        public void CreateDialog(string dialogId, string system, bool hasMemory, bool streamMode)
        {
            xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection(new XFEGPTMessage[] { new XFEGPTMessage(dialogId, new GPTMessage("system", system)) }, streamMode, ChatGPTModel.gpt3point5turbo, XFEComProtocol.XFEFAST, hasMemory, 0.7));
        }
        /// <summary>
        /// 创建一个新的对话，使用默认通信协议（快速响应通讯协议）
        /// </summary>
        /// <param name="dialogId">对话的ID</param>
        /// <param name="system">系统消息</param>
        /// <param name="hasMemory">是否记录对话信息</param>
        /// <param name="streamMode">是否为流式输出</param>
        /// <param name="chatGPTModel">所用的ChatGPT模型</param>
        public void CreateDialog(string dialogId, string system, bool hasMemory, bool streamMode, ChatGPTModel chatGPTModel)
        {
            xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection(new XFEGPTMessage[] { new XFEGPTMessage(dialogId, new GPTMessage("system", system)) }, streamMode, chatGPTModel, XFEComProtocol.XFEFAST, hasMemory, 0.7));
        }
        /// <summary>
        /// 创建一个新的对话，使用默认通信协议（快速响应通讯协议）
        /// </summary>
        /// <param name="dialogId">对话的ID</param>
        /// <param name="system">系统消息</param>
        /// <param name="hasMemory">是否记录对话信息</param>
        /// <param name="streamMode">是否为流式输出</param>
        /// <param name="chatGPTModel">所用的ChatGPT模型</param>
        /// <param name="temperature">系统的Temperature</param>
        public void CreateDialog(string dialogId, string system, bool hasMemory, bool streamMode, ChatGPTModel chatGPTModel, double temperature)
        {
            xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection(new XFEGPTMessage[] { new XFEGPTMessage(dialogId, new GPTMessage("system", system)) }, streamMode, chatGPTModel, XFEComProtocol.XFEFAST, hasMemory, temperature));
        }
        /// <summary>
        /// 创建一个新的对话，自定义通讯协议
        /// </summary>
        /// <param name="dialogId">对话的ID</param>
        /// <param name="system">系统消息</param>
        /// <param name="hasMemory">是否记录对话信息</param>
        /// <param name="streamMode">是否为流式输出</param>
        /// <param name="chatGPTModel">所用的ChatGPT模型</param>
        /// <param name="comProtocol">通信协议</param>
        public void CreateDialog(string dialogId, string system, bool hasMemory, bool streamMode, ChatGPTModel chatGPTModel, XFEComProtocol comProtocol)
        {
            xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection(new XFEGPTMessage[] { new XFEGPTMessage(dialogId, new GPTMessage("system", system)) }, streamMode, chatGPTModel, comProtocol, hasMemory, 0.7));
        }
        /// <summary>
        /// 创建一个新的对话，自定义通讯协议
        /// </summary>
        /// <param name="dialogId">对话的ID</param>
        /// <param name="system">系统消息</param>
        /// <param name="hasMemory">是否记录对话信息</param>
        /// <param name="streamMode">是否为流式输出</param>
        /// <param name="chatGPTModel">所用的ChatGPT模型</param>
        /// <param name="temperature">系统的Temperature</param>
        /// <param name="comProtocol">通信协议</param>
        public void CreateDialog(string dialogId, string system, bool hasMemory, bool streamMode, ChatGPTModel chatGPTModel, double temperature, XFEComProtocol comProtocol)
        {
            xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection(new XFEGPTMessage[] { new XFEGPTMessage(dialogId, new GPTMessage("system", system)) }, streamMode, chatGPTModel, comProtocol, hasMemory, temperature));
        }
        #endregion
        #region 插入对话
        /// <summary>
        /// 插入对话
        /// </summary>
        /// <param name="dialogId">要插入的对话ID</param>
        /// <param name="userMessage">用户对话内容</param>
        /// <param name="assistantMessage">AI对话的内容</param>
        /// <returns>插入的消息ID</returns>
        public string InsertDialog(string dialogId, string userMessage, string assistantMessage)
        {
            string messageId = Guid.NewGuid().ToString();
            xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId, new GPTMessage("user", userMessage)));
            xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId + "_Assistant", new GPTMessage("assistant", assistantMessage)));
            return messageId;
        }
        /// <summary>
        /// 插入对话
        /// </summary>
        /// <param name="dialogId">要插入的对话ID</param>
        /// <param name="messageId">插入的消息ID</param>
        /// <param name="userMessage">用户对话内容</param>
        /// <param name="assistantMessage">AI对话的内容</param>
        public void InsertDialog(string dialogId, string messageId, string userMessage, string assistantMessage)
        {
            xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId, new GPTMessage("user", userMessage)));
            xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId + "_Assistant", new GPTMessage("assistant", assistantMessage)));
        }
        /// <summary>
        /// 插入对话
        /// </summary>
        /// <param name="dialogId">要插入的对话ID</param>
        /// <param name="dialogMessage">对话，以二维数组的形式</param>
        /// <returns></returns>
        public string[] InsertDialog(string dialogId, string[,] dialogMessage)
        {
            try
            {
                if (dialogMessage.GetLength(1) != 2)
                {
                    throw new XFEChatGPTException("dialogMessage的格式不正确！应为string[任意数量,2]");
                }
                else if (dialogMessage.Length % 2 == 1)
                {
                    throw new XFEChatGPTException("dialogMessage不合法！只能成对添加user和assistant");
                }
                else
                {
                    string[] messageId = new string[dialogMessage.Length];
                    for (int i = 0; i < dialogMessage.Length; i += 2)
                    {
                        messageId[i] = Guid.NewGuid().ToString();
                        messageId[i + 1] = messageId[i] + "_Assistant";
                        xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId[i], new GPTMessage("user", dialogMessage[i == 0 ? 0 : i / 2, 0])));
                        xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId[i + 1], new GPTMessage("assistant", dialogMessage[i == 0 ? 0 : i / 2, 1])));
                    }
                    return messageId;
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new XFEChatGPTException("dialogMessage的格式不正确！应为string[任意数量,2]", ex);
            }
        }
        /// <summary>
        /// 插入对话
        /// </summary>
        /// <param name="dialogId">要插入的对话ID</param>
        /// <param name="dialogMessage">对话</param>
        /// <returns></returns>
        /// <exception cref="XFEChatGPTException"></exception>
        public string[] InsertDialog(string dialogId, params string[] dialogMessage)
        {
            try
            {
                if (dialogMessage.Length % 2 == 1)
                {
                    throw new XFEChatGPTException("dialogMessage不合法！只能成对添加user和assistant");
                }
                else
                {
                    string[] messageId = new string[dialogMessage.Length];
                    for (int i = 0; i < dialogMessage.Length; i += 2)
                    {
                        messageId[i] = Guid.NewGuid().ToString();
                        messageId[i + 1] = messageId[i] + "_Assistant";
                        xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId[i], new GPTMessage("user", dialogMessage[i])));
                        xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId[i + 1], new GPTMessage("assistant", dialogMessage[i + 1])));
                    }
                    return messageId;
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new XFEChatGPTException("dialogMessage的格式不正确！应为string[任意数量,2]", ex);
            }
        }
        /// <summary>
        /// 插入对话
        /// </summary>
        /// <param name="dialogId">要插入的对话ID</param>
        /// <param name="messageId">对话的消息ID，以数组的形式</param>
        /// <param name="dialogMessage">对话，以二维数组的形式</param>
        /// <exception cref="XFEChatGPTException"></exception>
        public void InsertDialog(string dialogId, string[] messageId, string[,] dialogMessage)
        {
            try
            {
                if (messageId.Length != dialogMessage.Length)
                {
                    throw new XFEChatGPTException("messageId与dialogMessage数量不对应");
                }
                else if (dialogMessage.GetLength(1) != 2)
                {
                    throw new XFEChatGPTException("dialogMessage的格式不正确！应为string[任意数量,2]");
                }
                else if (dialogMessage.Length % 2 == 1)
                {
                    throw new XFEChatGPTException("dialogMessage不合法！只能成对添加user和assistant");
                }
                else
                {
                    for (int i = 0; i < dialogMessage.Length; i += 2)
                    {
                        xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId[i], new GPTMessage("user", dialogMessage[i == 0 ? 0 : i / 2, 0])));
                        xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId[i + 1], new GPTMessage("assistant", dialogMessage[i == 0 ? 0 : i / 2, 1])));
                    }
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new Exception("dialogMessage的格式不正确！应为string[任意数量,2]", ex);
            }
        }
        /// <summary>
        /// 插入对话
        /// </summary>
        /// <param name="dialogId">要插入的对话ID</param>
        /// <param name="messageId">对话的消息ID，以数组的形式</param>
        /// <param name="dialogMessage">对话，以一维数组的形式</param>
        /// <exception cref="XFEChatGPTException"></exception>
        public void InsertDialog(string dialogId, string[] messageId, string[] dialogMessage)
        {
            try
            {
                if (messageId.Length != dialogMessage.Length)
                {
                    throw new XFEChatGPTException("messageId与dialogMessage数量不对应");
                }
                else if (dialogMessage.Length % 2 == 1)
                {
                    throw new XFEChatGPTException("dialogMessage不合法！只能成对添加user和assistant");
                }
                else
                {
                    for (int i = 0; i < dialogMessage.Length; i += 2)
                    {
                        xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId[i], new GPTMessage("user", dialogMessage[i])));
                        xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId[i + 1], new GPTMessage("assistant", dialogMessage[i + 1])));
                    }
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new XFEChatGPTException("dialogMessage的格式不正确！应为string[任意数量,2]", ex);
            }
        }
        /// <summary>
        /// 插入对话，自动补全对话
        /// </summary>
        /// <param name="dialogId">要插入的对话ID</param>
        /// <param name="dialogMessage">对话</param>
        /// <returns></returns>
        /// <exception cref="XFEChatGPTException"></exception>
        public string[] InsertDialogAutoComplete(string dialogId, params string[] dialogMessage)
        {
            try
            {
                string[] messageId = dialogMessage.Length % 2 == 0 ? new string[dialogMessage.Length] : new string[dialogMessage.Length + 1];
                for (int i = 0; i < dialogMessage.Length; i += 2)
                {
                    messageId[i] = Guid.NewGuid().ToString();
                    messageId[i + 1] = messageId[i] + "_Assistant";
                    xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId[i], new GPTMessage("user", dialogMessage[i])));
                    xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId[i + 1], new GPTMessage("assistant", dialogMessage.Length > i + 1 ? dialogMessage[i + 1] : "该条问题未回复")));
                }
                return messageId;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new XFEChatGPTException("dialogMessage的格式不正确！", ex);
            }
        }
        #endregion
        #region 其余操作
        /// <summary>
        /// 获取对话
        /// </summary>
        /// <param name="dialogId">对话ID</param>
        /// <returns></returns>
        public GPTMessage[] GetDialog(string dialogId)
        {
            return xFEGPTMemoryDialog[dialogId].GetGPTMessages();
        }
        /// <summary>
        /// 以字符串数组的形式获取对话
        /// </summary>
        /// <param name="dialogId"></param>
        /// <returns></returns>
        public string[] GetDialogStrings(string dialogId)
        {
            return xFEGPTMemoryDialog[dialogId].GetGPTMessageStrings();
        }
        #endregion
        #region 开始对话
        /// <summary>
        /// 开始对话
        /// </summary>
        /// <param name="dialogId">对话ID</param>
        /// <param name="messageId">本消息ID</param>
        /// <param name="askMessage">询问的内容</param>
        public void AskChatGPT(string dialogId, string messageId, string askMessage)
        {
            if (xFEGPTMemoryDialog[dialogId].GetGPTMessages().CheckLegal())
            {
                //判断如果最后一条消息是用户消息，则在添加新的用户消息前添加一条助手消息然后再添加用户消息，否则直接添加用户消息
                if (xFEGPTMemoryDialog[dialogId].GetLastRole() == "user")
                {
                    xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId + "_Assistant", new GPTMessage("assistant", "正在思考中...")));
                    xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId, new GPTMessage("user", askMessage)));
                }
                else
                {
                    xFEGPTMemoryDialog[dialogId].Add(new XFEGPTMessage(messageId, new GPTMessage("user", askMessage)));
                }
                var nowThread = new Thread(StartGetGPTMessage);
                nowThread.Start(new MessageIdDialogIdAndThread(messageId, dialogId, nowThread));
            }
            else
            {
                throw new XFEChatGPTException("对话信息不合法");
            }
        }
        #endregion
        #region 停止对话
        //TODO:
        #endregion
        #endregion
        #region 线程方法
        private async void StartGetGPTMessage(object sender)
        {
            var messageIdAndThread = (MessageIdDialogIdAndThread)sender;
            string dialogId = messageIdAndThread.dialogId;
            string messageId = messageIdAndThread.messageId;
            Thread thread = messageIdAndThread.thread;
            var nowDialog = xFEGPTMemoryDialog[dialogId];
            if (nowDialog.StreamMode)
            {
                bool isStarted = false;
                try
                {
                    #region 进行HTTP请求
                    ClientWebSocket webSocket = new ClientWebSocket();
                    await webSocket.ConnectAsync(new Uri("ws://gpt.api.xfegzs.com/"), CancellationToken.None);
                    if (nowDialog.MemorableMode)
                    {
                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(nowDialog.CreateAskMessage().ConvertToJson())), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else
                    {
                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(new XFEAskGPTMessage(false, nowDialog.StreamMode, nowDialog.ChatGPTModel.GetModelString(), null, nowDialog.XFEComProtocol, nowDialog.System, nowDialog[nowDialog.Count - 1].gPTMessage.Content).ConvertToJson())), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    #endregion
                    while (true)
                    {
                        #region 读取消息
                        byte[] buffer = new byte[1024];
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);//接收消息
                        string nowReceivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);//将消息转换为字符串
                        #endregion
                        if (nowDialog.XFEComProtocol == XFEComProtocol.XFEFAST)
                        {
                            if (nowReceivedMessage == "[DONE]")
                            {
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFEDONE]", messageId, GenerateState.End, dialogId));
                                break;
                            }
                            else if (nowReceivedMessage.Contains("[XFERemoteAPIError]"))
                            {
                                if (!isStarted)
                                {
                                    XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start, dialogId));
                                    xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", true);
                                }
                                else
                                {
                                    xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", false);
                                }
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error, dialogId));
                            }
                            else if (nowReceivedMessage == "[XFE]")
                            {
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start, dialogId));
                                isStarted = true;
                                xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", string.Empty, true);
                            }
                            else
                            {
                                xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", nowReceivedMessage, false);
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Continue, dialogId));
                            }
                        }
                        else
                        {
                            if (nowReceivedMessage == "[DONE]")
                            {
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFEDONE]", messageId, GenerateState.End, dialogId));
                                break;
                            }
                            else if (nowReceivedMessage.Contains("[XFERemoteAPIError]"))
                            {
                                if (!isStarted)
                                {
                                    XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start, dialogId));
                                    xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", true);
                                }
                                else
                                {
                                    xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", false);
                                }
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error, dialogId));
                            }
                            else if (nowReceivedMessage == "[XFE]")
                            {
                                XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start, dialogId));
                                isStarted = true;
                                xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", string.Empty, true);
                            }
                            else
                            {
                                if (nowReceivedMessage.Contains("[XFE]"))
                                {
                                    nowReceivedMessage = nowReceivedMessage.Replace("[XFE]", string.Empty);
                                    xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", nowReceivedMessage, false);
                                    XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Continue, dialogId));
                                }
                                else
                                {
                                    if (!isStarted)
                                    {
                                        XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start, dialogId));
                                        xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", true);
                                    }
                                    else
                                    {
                                        xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", false);
                                    }
                                }
                            }
                        }
                    }
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed.", CancellationToken.None);
                }
                catch (Exception ex) when (!(ex is XFEChatGPTException))
                {
                    if (!isStarted)
                    {
                        XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start, dialogId));
                        xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", true);
                    }
                    else
                    {
                        xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", false);
                    }
                    XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(ex.ToString(), messageId, GenerateState.Error, dialogId));
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                try
                {
                    #region 进行HTTP请求
                    ClientWebSocket webSocket = new ClientWebSocket();
                    await webSocket.ConnectAsync(new Uri("ws://gpt.api.xfegzs.com/"), CancellationToken.None);
                    if (nowDialog.MemorableMode)
                    {
                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(nowDialog.CreateAskMessage().ConvertToJson())), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else
                    {
                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(new XFEAskGPTMessage(false, nowDialog.StreamMode, nowDialog.ChatGPTModel.GetModelString(), null, nowDialog.XFEComProtocol, nowDialog.System, nowDialog[nowDialog.Count - 1].gPTMessage.Content).ConvertToJson())), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    #endregion
                    #region 读取消息
                    byte[] buffer = new byte[1024];
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);//接收消息
                    string nowReceivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);//将消息转换为字符串
                    #endregion
                    if (nowReceivedMessage.Contains("[XFE]"))
                    {
                        nowReceivedMessage = nowReceivedMessage.Replace("[XFE]", string.Empty);
                        xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", nowReceivedMessage, true);
                        XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Start, dialogId));
                    }
                    else
                    {
                        xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", nowReceivedMessage, true);
                        XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error, dialogId));
                    }
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed.", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", ex.ToString(), true);
                    XFEChatGPTMessageReceived.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(ex.ToString(), messageId, GenerateState.Error, dialogId));
                    Console.WriteLine(ex.ToString());
                }
            }
            threadList.Remove(thread);
        }
        #endregion
        #region 公有事件
        /// <summary>
        /// 当收到ChatGPT的消息时触发
        /// </summary>
        public event EventHandler<MemorableGPTMessageReceivedEventArgs> XFEChatGPTMessageReceived;
        #endregion
        #region 构造方法
        /// <summary>
        /// 创建一个有记忆功能的XFEChatGPT对象
        /// </summary>
        public MemorableXFEChatGPT()
        {
            xFEGPTMemoryDialog = new PrivateXFEGPTMemoryDialog();
        }
        #endregion
    }
    /// <summary>
    /// XFEChatGPT的扩展方法
    /// </summary>
    public static class XFEChatGPTExtension
    {
        /// <summary>
        /// 检查是否合法（system在最前，接着是user，然后是Assistant，以此类推...，不能连着2个user或者Assistant）
        /// </summary>
        /// <returns>是否合法</returns>
        public static bool CheckLegal(this GPTMessage[] xFEGPTMessages)
        {
            if (xFEGPTMessages.Length == 0)
            {
                return true;
            }
            else
            {
                if (xFEGPTMessages[0].Role != "system")
                {
                    return false;
                }
                else
                {
                    string lastRole = xFEGPTMessages[0].Role;
                    for (int i = 1; i < xFEGPTMessages.Length; i++)
                    {
                        if (xFEGPTMessages[i].Role == lastRole)
                        {
                            return false;
                        }
                        else
                        {
                            lastRole = xFEGPTMessages[i].Role;
                        }
                    }
                    return true;
                }
            }
        }
    }
    #region ChatGPT类
    #region ChatGPT默认类
    /// <summary>
    /// 接收到的GPT消息
    /// </summary>
    public abstract class ReceivedGPTMessage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string id { get; private set; }
        /// <summary>
        /// 消息对象
        /// </summary>
        public string Object { get; private set; }
        /// <summary>
        /// 消息创建时间
        /// </summary>
        public long created { get; private set; }
        /// <summary>
        /// 使用的GPT模型
        /// </summary>
        public string model { get; private set; }
        /// <summary>
        /// Token令牌的使用情况
        /// </summary>
        public TokenUsage usage { get; private set; }
        /// <summary>
        /// GPT返回的具体数据（数组的形式）
        /// </summary>
        public MessageChoice[] choices { get; private set; }
        /// <summary>
        /// 接收到的GPT消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="object"></param>
        /// <param name="created"></param>
        /// <param name="model"></param>
        /// <param name="usage"></param>
        /// <param name="choices"></param>
        public ReceivedGPTMessage(string id, string @object, long created, string model, TokenUsage usage, MessageChoice[] choices)
        {
            this.id = id;
            Object = @object;
            this.created = created;
            this.model = model;
            this.usage = usage;
            this.choices = choices;
        }
    }
    /// <summary>
    /// GPT消息链的组成单元
    /// </summary>
    public abstract class MessageChoice
    {
        /// <summary>
        /// Delta消息（用于流式接收）
        /// </summary>
        public GPTMessage delta { get; private set; }
        /// <summary>
        /// 单个消息（用于单个接收）
        /// </summary>
        public GPTMessage message { get; private set; }
        /// <summary>
        /// 结束原因
        /// </summary>
        public string finish_reason { get; private set; }
        /// <summary>
        /// 消息索引
        /// </summary>
        public int index { get; private set; }
        /// <summary>
        /// GPT消息链的组成单元
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="message"></param>
        /// <param name="finish_reason"></param>
        /// <param name="index"></param>
        public MessageChoice(GPTMessage delta, GPTMessage message, string finish_reason, int index)
        {
            this.delta = delta;
            this.message = message;
            this.finish_reason = finish_reason;
            this.index = index;
        }
        /// <summary>
        /// GPT消息链的组成单元
        /// </summary>
        /// <param name="message">GPTMessage消息对象</param>
        /// <param name="finish_reason">结束原因</param>
        /// <param name="index">消息索引</param>
        public MessageChoice(GPTMessage message, string finish_reason, int index)
        {
            this.message = message;
            this.finish_reason = finish_reason;
            this.index = index;
        }
    }
    /// <summary>
    /// Token令牌的使用情况
    /// </summary>
    public class TokenUsage
    {
        /// <summary>
        /// 用于提示的Token令牌
        /// </summary>
        public int prompt_tokens { get; private set; }
        /// <summary>
        /// 用于完成的Token令牌
        /// </summary>
        public int completion_tokens { get; private set; }
        /// <summary>
        /// 总共使用的Token令牌
        /// </summary>
        public int total_tokens { get; private set; }
        /// <summary>
        /// Token令牌的使用情况
        /// </summary>
        /// <param name="prompt_tokens"></param>
        /// <param name="completion_tokens"></param>
        /// <param name="total_tokens"></param>
        public TokenUsage(int prompt_tokens, int completion_tokens, int total_tokens)
        {
            this.prompt_tokens = prompt_tokens;
            this.completion_tokens = completion_tokens;
            this.total_tokens = total_tokens;
        }
    }
    /// <summary>
    /// GPT环境数据
    /// </summary>
    public class EnvironmentGPTData
    {
        /// <summary>
        /// 使用的GPT模型
        /// </summary>
        public string model { get; set; }
        /// <summary>
        /// 是否为流式接收
        /// </summary>
        public bool stream { get; set; }
        /// <summary>
        /// 温度，此处为GPT的创造性
        /// </summary>
        public double temperature { get; set; }
        /// <summary>
        /// 发送给GPT的消息
        /// </summary>
        public GPTMessage[] messages { get; set; }
        /// <summary>
        /// GPT环境数据
        /// </summary>
        /// <param name="model"></param>
        /// <param name="stream"></param>
        /// <param name="temperature"></param>
        /// <param name="messages"></param>
        public EnvironmentGPTData(string model, bool stream, double temperature, GPTMessage[] messages)
        {
            this.model = model;
            this.stream = stream;
            this.temperature = temperature;
            this.messages = messages;
        }
    }
    /// <summary>
    /// GPT消息
    /// </summary>
    public class GPTMessage
    {
        private string role;
        /// <summary>
        /// 角色（目前已知有：System，User和Assistant）
        /// </summary>
        public string Role
        {
            get
            {
                return role;
            }
            set
            {
                role = SetRoleAndCheckRoleLegal(value);
            }
        }
        /// <summary>
        /// 与角色对应的消息内容
        /// </summary>
        public string Content { get; set; }
        private string SetRoleAndCheckRoleLegal(string inputRole)
        {
            string lowerRole = inputRole.ToLower();
            if (lowerRole == "system" || lowerRole == "user" || lowerRole == "assistant")
            {
                return lowerRole;
            }
            else
            {
                throw new XFEChatGPTException($"角色名称不合法：{inputRole}\n应为system，user或assistant三者中的一个");
            }
        }
        /// <summary>
        /// GPT消息
        /// </summary>
        /// <param name="role">角色</param>
        /// <param name="content">内容</param>
        public GPTMessage(string role, string content)
        {
            this.role = role;
            this.Content = content;
        }
    }
    #endregion
    #region ChatGPT自定义类
    class XFEAskGPTMessage
    {
        public bool isSelfEditData { get; set; }
        public bool stream { get; set; }
        public string chatGPTModel { get; set; }
        public EnvironmentGPTData EnvironmentGPTData { get; set; }
        public XFEComProtocol comProtocol { get; set; }
        public string systemContent { get; set; }
        public string askContent { get; set; }
        public XFEAskGPTMessage(bool isSelfEditData, bool stream, string chatGPTModel, EnvironmentGPTData EnvironmentGPTData, XFEComProtocol comProtocol, string systemContent, string askContent)
        {
            this.isSelfEditData = isSelfEditData;
            this.stream = stream;
            this.chatGPTModel = chatGPTModel;
            this.EnvironmentGPTData = EnvironmentGPTData;
            this.comProtocol = comProtocol;
            this.systemContent = systemContent;
            this.askContent = askContent;
        }
        public XFEAskGPTMessage() { }
    }
    /// <summary>
    /// 从服务器接收到的GPT消息
    /// </summary>
    public abstract class XFEGPTMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 消息接收器ID
        /// </summary>
        public string messageId { get; set; }
        /// <summary>
        /// 生成状态
        /// </summary>
        public GenerateState generateState { get; set; }
        /// <summary>
        /// 从服务器接收到的GPT消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageId"></param>
        /// <param name="generateState"></param>
        public XFEGPTMessageReceivedEventArgs(string message, string messageId, GenerateState generateState)
        {
            this.message = message;
            this.messageId = messageId;
            this.generateState = generateState;
        }
    }
    class PrivateXFEGPTMessageReceivedEventArgs : XFEGPTMessageReceivedEventArgs
    {
        public PrivateXFEGPTMessageReceivedEventArgs(string message, string id, GenerateState generateState) : base(message, id, generateState)
        {

        }
    }
    /// <summary>
    /// 从服务器接收到的GPT消息（记忆模式）
    /// </summary>
    public abstract class MemorableGPTMessageReceivedEventArgs : XFEGPTMessageReceivedEventArgs
    {
        /// <summary>
        /// 对话ID
        /// </summary>
        public string dialogId { get; set; }
        /// <summary>
        /// 从服务器接收到的GPT消息（记忆模式）
        /// </summary>
        /// <param name="message"></param>
        /// <param name="id"></param>
        /// <param name="generateState"></param>
        /// <param name="dialogId"></param>
        public MemorableGPTMessageReceivedEventArgs(string message, string id, GenerateState generateState, string dialogId) : base(message, id, generateState)
        {
            this.dialogId = dialogId;
        }
    }
    class PrivateMemorableGPTMessageReceivedEventArgs : MemorableGPTMessageReceivedEventArgs
    {
        public PrivateMemorableGPTMessageReceivedEventArgs(string message, string id, GenerateState generateState, string dialogId) : base(message, id, generateState, dialogId)
        {

        }
    }
    class PrivateMessageChoice : MessageChoice
    {
        public PrivateMessageChoice(GPTMessage delta, GPTMessage message, string finish_reason, int index) : base(delta, message, finish_reason, index)
        {

        }
        public PrivateMessageChoice(GPTMessage message, string finish_reason, int index) : base(message, finish_reason, index)
        {

        }
    }
    class PrivateReceivedGPTMessage : ReceivedGPTMessage
    {
        public PrivateReceivedGPTMessage(string id, string @object, long created, string model, TokenUsage usage, PrivateMessageChoice[] choices) : base(id, @object, created, model, usage, choices)
        {

        }
    }
    /// <summary>
    /// XFEGPT消息（具有消息ID）
    /// </summary>
    public class XFEGPTMessage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string messageId { get; set; }
        /// <summary>
        /// GPT消息
        /// </summary>
        public GPTMessage gPTMessage { get; set; }
        /// <summary>
        /// XFEGPT消息（具有消息ID）
        /// </summary>
        /// <param name="messageId">消息ID</param>
        /// <param name="gPTMessage">GPT消息</param>
        public XFEGPTMessage(string messageId, GPTMessage gPTMessage)
        {
            this.messageId = messageId;
            this.gPTMessage = gPTMessage;
        }
    }
    /// <summary>
    /// XFEGPT的对话
    /// </summary>
    public class XFEGPTMessageCollection : IEnumerable<XFEGPTMessage>
    {
        #region 属性
        private readonly List<XFEGPTMessage> xFEGPTMessages;
        /// <summary>
        /// 消息数量
        /// </summary>
        public int Count
        {
            get
            {
                return xFEGPTMessages.Count;
            }
        }
        /// <summary>
        /// XFE通讯协议
        /// </summary>
        public XFEComProtocol XFEComProtocol { get; set; }
        /// <summary>
        /// 使用的模型
        /// </summary>
        public ChatGPTModel ChatGPTModel { get; set; }
        /// <summary>
        /// 是否为流式输出
        /// </summary>
        public bool StreamMode { get; set; }
        /// <summary>
        /// 是否为记忆模式
        /// </summary>
        public bool MemorableMode { get; set; }
        /// <summary>
        /// 对话的系统设置
        /// </summary>
        public string System
        {
            get
            {
                if (xFEGPTMessages.Count > 0 && xFEGPTMessages[0].gPTMessage.Role == "system")
                {
                    return xFEGPTMessages[0].gPTMessage.Content;
                }
                else
                {
                    throw new XFEChatGPTException("消息集合中没有系统消息！");
                }
            }
            set
            {
                if (xFEGPTMessages.Count > 0 && xFEGPTMessages[0].gPTMessage.Role == "system")
                {
                    xFEGPTMessages[0].gPTMessage.Content = value;
                }
                else
                {
                    throw new XFEChatGPTException("消息集合中没有系统消息！");
                }
            }
        }
        /// <summary>
        /// 对话的Temperature
        /// </summary>
        public double Temperature { get; set; }
        #endregion
        #region 外部方法
        /// <summary>
        /// 添加XFEGPT消息
        /// </summary>
        /// <param name="xFEGPTMessage"></param>
        public void Add(XFEGPTMessage xFEGPTMessage)
        {
            xFEGPTMessages.Add(xFEGPTMessage);
        }
        /// <summary>
        /// 添加XFEGPT消息
        /// </summary>
        /// <param name="xFEGPTMessages"></param>
        public void AddRange(XFEGPTMessage[] xFEGPTMessages)
        {
            this.xFEGPTMessages.AddRange(xFEGPTMessages);
        }
        /// <summary>
        /// 根据指定的ID移除XFEGPT消息
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            xFEGPTMessages.RemoveAt(index);
        }
        /// <summary>
        /// 移除XFEGPT消息
        /// </summary>
        /// <param name="xFEGPTMessage"></param>
        public void Remove(XFEGPTMessage xFEGPTMessage)
        {
            xFEGPTMessages.Remove(xFEGPTMessage);
        }
        /// <summary>
        /// 根据指定的ID在其后插入XFEGPT消息
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="xFEGPTMessage"></param>
        /// <exception cref="XFEChatGPTException"></exception>
        public void InsertByMessageId(string messageId, XFEGPTMessage xFEGPTMessage)
        {
            for (int i = 0; i < xFEGPTMessages.Count; i++)
            {
                if (xFEGPTMessages[i].messageId == messageId)
                {
                    xFEGPTMessages.Insert(i, xFEGPTMessage);
                    return;
                }
            }
            throw new XFEChatGPTException("消息集合中没有指定ID的消息！");
        }
        /// <summary>
        /// 清除XFEGPT消息
        /// </summary>
        public void Clear()
        {
            xFEGPTMessages.Clear();
        }
        /// <summary>
        /// 通过消息ID获取XFEGPT消息
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public XFEGPTMessage GetXFEGPTMessageByMessageId(string messageId)
        {
            foreach (XFEGPTMessage xFEGPTMessage in xFEGPTMessages)
            {
                if (xFEGPTMessage.messageId == messageId)
                {
                    return xFEGPTMessage;
                }
            }
            return null;
        }
        /// <summary>
        /// 获取本集合的GPTMessage数组
        /// </summary>
        /// <returns></returns>
        public GPTMessage[] GetGPTMessages()
        {
            List<GPTMessage> gPTMessages = new List<GPTMessage>();
            foreach (XFEGPTMessage xFEGPTMessage in xFEGPTMessages)
            {
                gPTMessages.Add(xFEGPTMessage.gPTMessage);
            }
            return gPTMessages.ToArray();
        }
        /// <summary>
        /// 以字符串的形式获取本集合的GPTMessage数组
        /// </summary>
        /// <returns></returns>
        public string[] GetGPTMessageStrings()
        {
            List<string> gPTMessages = new List<string>();
            foreach (XFEGPTMessage xFEGPTMessage in xFEGPTMessages)
            {
                gPTMessages.Add(xFEGPTMessage.gPTMessage.Content);
            }
            return gPTMessages.ToArray();
        }
        /// <summary>
        /// 对本集合进行索引器操作
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns></returns>
        public XFEGPTMessage this[int index]
        {
            get
            {
                return xFEGPTMessages[index];
            }
            set
            {
                xFEGPTMessages[index] = value;
            }
        }
        /// <summary>
        /// 获取最后一个角色
        /// </summary>
        /// <returns>最后一个角色</returns>
        public string GetLastRole()
        {
            return xFEGPTMessages[Count - 1].gPTMessage.Role;
        }
        internal XFEAskGPTMessage CreateAskMessage()
        {
            return new XFEAskGPTMessage(true, StreamMode, ChatGPTModel.GetModelString(), new EnvironmentGPTData(ChatGPTModel.GetModelString(), StreamMode, Temperature, GetGPTMessages()), XFEComProtocol, string.Empty, string.Empty);
        }
        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<XFEGPTMessage> GetEnumerator()
        {
            return xFEGPTMessages.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return xFEGPTMessages.GetEnumerator();
        }
        #endregion
        #region 构造方法
        /// <summary>
        /// XFEGPT的对话
        /// </summary>
        /// <param name="xFEGPTMessages"></param>
        /// <param name="memorableMode"></param>
        /// <param name="chatGPTModel"></param>
        /// <param name="comProtocol"></param>
        /// <param name="streamMode"></param>
        /// <param name="temperature"></param>
        public XFEGPTMessageCollection(XFEGPTMessage[] xFEGPTMessages, bool memorableMode, ChatGPTModel chatGPTModel, XFEComProtocol comProtocol, bool streamMode, double temperature)
        {
            this.xFEGPTMessages = new List<XFEGPTMessage>(xFEGPTMessages);
            MemorableMode = memorableMode;
            XFEComProtocol = comProtocol;
            ChatGPTModel = chatGPTModel;
            StreamMode = streamMode;
            Temperature = temperature;
        }
        /// <summary>
        /// XFEGPT的对话
        /// </summary>
        internal XFEGPTMessageCollection()
        {
            xFEGPTMessages = new List<XFEGPTMessage>();
        }
        #endregion
    }
    /// <summary>
    /// GPT对话记录
    /// </summary>
    public abstract class XFEGPTMemoryDialog : IEnumerable<XFEGPTMessage>
    {
        private Dictionary<string, XFEGPTMessageCollection> gPTMessageWithId;
        internal XFEGPTMemoryDialog()
        {
            gPTMessageWithId = new Dictionary<string, XFEGPTMessageCollection>();
        }
        #region 操作部分
        /// <summary>
        /// 通过对话ID操作对话
        /// </summary>
        /// <param name="dialogId">对话的ID</param>
        /// <returns></returns>
        public XFEGPTMessageCollection this[string dialogId]
        {
            get
            {
                try
                {
                    return gPTMessageWithId[dialogId];
                }
                catch (Exception ex)
                {
                    throw new XFEChatGPTException("查找指定ID的对话时出错！", ex);
                }
            }
            set
            {
                try
                {
                    gPTMessageWithId[dialogId] = value;
                }
                catch (Exception ex)
                {
                    throw new XFEChatGPTException("设置指定ID的对话时出错！", ex);
                }
            }
        }
        /// <summary>
        /// 添加一个对话记录
        /// </summary>
        /// <param name="dialogId">对话ID</param>
        /// <param name="xFEGPTMessages">XFEGPT的对话</param>
        public void Add(string dialogId, XFEGPTMessageCollection xFEGPTMessages)
        {
            gPTMessageWithId.Add(dialogId, xFEGPTMessages);
        }
        /// <summary>
        /// 添加多个对话记录
        /// </summary>
        /// <param name="messages">ID和XFEGPT的对话的键值对</param>
        public void AddRange(KeyValuePair<string, XFEGPTMessageCollection>[] messages)
        {
            foreach (var item in messages)
            {
                gPTMessageWithId.Add(item.Key, item.Value);
            }
        }
        /// <summary>
        /// 移除一个对话记录
        /// </summary>
        /// <param name="dialogId">对话ID</param>
        public void Remove(string dialogId)
        {
            gPTMessageWithId.Remove(dialogId);
        }
        /// <summary>
        /// 移除多个对话记录
        /// </summary>
        /// <param name="ids">对话ID的数组</param>
        public void RemoveRange(string[] ids)
        {
            foreach (var item in ids)
            {
                gPTMessageWithId.Remove(item);
            }
        }
        /// <summary>
        /// 清空所有对话的对话记录
        /// </summary>
        public void Clear()
        {
            gPTMessageWithId.Clear();
        }
        /// <summary>
        /// 更新指定对话ID的对话记录
        /// </summary>
        /// <param name="dialogId">对话ID</param>
        /// <param name="messages">XFEGPT的对话</param>
        public void Update(string dialogId, XFEGPTMessageCollection messages)
        {
            gPTMessageWithId[dialogId] = messages;
        }
        /// <summary>
        /// 实时更新对话记录
        /// </summary>
        /// <param name="dialogId"></param>
        /// <param name="messageId"></param>
        /// <param name="nowMessage"></param>
        /// <param name="clear"></param>
        public void InstanceUpdate(string dialogId, string messageId, string nowMessage, bool clear)
        {
            var xFEGPTMessages = gPTMessageWithId[dialogId];
            if (xFEGPTMessages.GetXFEGPTMessageByMessageId(messageId) is null)
            {
                xFEGPTMessages.Add(new XFEGPTMessage(messageId, new GPTMessage("assistant", nowMessage)));
            }
            else
            {
                if (xFEGPTMessages.GetLastRole() == "system")
                {
                    throw new XFEChatGPTException("系统消息无法更改");
                }
                else
                {
                    if (clear)
                    {
                        xFEGPTMessages.GetXFEGPTMessageByMessageId(messageId).gPTMessage.Content = nowMessage;
                    }
                    else
                    {
                        xFEGPTMessages.GetXFEGPTMessageByMessageId(messageId).gPTMessage.Content += nowMessage;
                    }
                }
            }
        }
        #endregion
        #region 获取部分
        /// <summary>
        /// 判断是否包含某个对话
        /// </summary>
        /// <param name="dialogId">对话ID</param>
        /// <returns>是否含有该对话</returns>
        public bool Contains(string dialogId)
        {
            return gPTMessageWithId.ContainsKey(dialogId);
        }
        /// <summary>
        /// 获取对话记录
        /// </summary>
        /// <param name="dialogId">对话ID</param>
        /// <returns></returns>
        public XFEGPTMessageCollection GetXFEGPTMessages(string dialogId)
        {
            return gPTMessageWithId[dialogId];
        }
        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<XFEGPTMessage> GetEnumerator()
        {
            foreach (var item in gPTMessageWithId)
            {
                foreach (var item2 in item.Value)
                {
                    yield return item2;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
    class PrivateXFEGPTMemoryDialog : XFEGPTMemoryDialog
    {
        public PrivateXFEGPTMemoryDialog() { }
    }
    /// <summary>
    /// API密钥
    /// </summary>
    public class ApiKey
    {
        /// <summary>
        /// APIKEY
        /// </summary>
        public string APIKey { get; set; }
        /// <summary>
        /// APIKey的描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{{[+-{Description}-+][+-{APIKey}-+]}}";
        }
        /// <summary>
        /// 将字符串转换为ApiKey的List
        /// </summary>
        /// <param name="keyString"></param>
        /// <returns></returns>
        public static List<ApiKey> ToApiKey(string keyString)
        {
            string[] keyGroup = keyString.Split(new char[] { '{', '}' });
            List<ApiKey> apiKeys = new List<ApiKey>();
            foreach (string key in keyGroup)
            {
                string[] oneKey = key.Split(new string[] { "[+-", "-+]" }, StringSplitOptions.RemoveEmptyEntries);
                if (oneKey.Length == 2)
                {
                    apiKeys.Add(new ApiKey(oneKey[1], oneKey[0]));
                }
            }
            return apiKeys;
        }
        /// <summary>
        /// APIKey秘钥
        /// </summary>
        /// <param name="APIKey">秘钥</param>
        /// <param name="Description">描述</param>
        public ApiKey(string APIKey, string Description)
        {
            this.APIKey = APIKey;
            this.Description = Description;
        }
    }
    #endregion
    #endregion
    #region 其余类
    class MessageIdAndThread
    {
        public string messageId { get; set; }
        public Thread thread { get; set; }
        public MessageIdAndThread(string messageId, Thread thread)
        {
            this.messageId = messageId;
            this.thread = thread;
        }
    }
    class MessageIdDialogIdAndThread : MessageIdAndThread
    {
        public string dialogId { get; set; }
        public MessageIdDialogIdAndThread(string messageId, string dialogId, Thread thread) : base(messageId, thread)
        {
            this.dialogId = dialogId;
        }
    }
    #endregion
}