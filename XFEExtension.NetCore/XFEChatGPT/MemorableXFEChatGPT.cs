using System.Net.WebSockets;
using System.Text;
using XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;
using XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;
using XFEExtension.NetCore.XFEChatGPT.OtherInnerClass;
using XFEExtension.NetCore.XFETransform;

namespace XFEExtension.NetCore.XFEChatGPT;

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
        xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection([new XFEGPTMessage(dialogId, new GPTMessage("system", system))], false, ChatGPTModel.gpt3point5turbo, XFEComProtocol.XFEFAST, false, 0.7));
    }
    /// <summary>
    /// 创建一个新的对话，使用默认通信协议（快速响应通讯协议）
    /// </summary>
    /// <param name="dialogId">对话的ID</param>
    /// <param name="system">系统消息</param>
    /// <param name="hasMemory">是否记录对话信息</param>
    public void CreateDialog(string dialogId, string system, bool hasMemory)
    {
        xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection([new XFEGPTMessage(dialogId, new GPTMessage("system", system))], false, ChatGPTModel.gpt3point5turbo, XFEComProtocol.XFEFAST, hasMemory, 0.7));
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
        xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection([new XFEGPTMessage(dialogId, new GPTMessage("system", system))], hasMemory, ChatGPTModel.gpt3point5turbo, XFEComProtocol.XFEFAST, streamMode, 0.7));
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
        xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection([new XFEGPTMessage(dialogId, new GPTMessage("system", system))], hasMemory, chatGPTModel, XFEComProtocol.XFEFAST, streamMode, 0.7));
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
        xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection([new XFEGPTMessage(dialogId, new GPTMessage("system", system))], hasMemory, chatGPTModel, XFEComProtocol.XFEFAST, streamMode, temperature));
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
        xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection([new XFEGPTMessage(dialogId, new GPTMessage("system", system))], hasMemory, chatGPTModel, comProtocol, streamMode, 0.7));
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
        xFEGPTMemoryDialog.Add(dialogId, new XFEGPTMessageCollection([new XFEGPTMessage(dialogId, new GPTMessage("system", system))], hasMemory, chatGPTModel, comProtocol, streamMode, temperature));
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
    public void StopChat()
    {

    }
    #endregion
    #endregion
    #region 线程方法
    private async void StartGetGPTMessage(object? sender)
    {
        var messageIdAndThread = (MessageIdDialogIdAndThread)sender!;
        string dialogId = messageIdAndThread.DialogId;
        string messageId = messageIdAndThread.MessageId;
        Thread thread = messageIdAndThread.Thread;
        var nowDialog = xFEGPTMemoryDialog[dialogId];
        if (nowDialog.StreamMode)
        {
            bool isStarted = false;
            try
            {
                #region 进行HTTP请求
                ClientWebSocket webSocket = new();
                await webSocket.ConnectAsync(new Uri("ws://gpt.api.xfegzs.com/"), CancellationToken.None);
                if (nowDialog.MemorableMode)
                {
                    var json = nowDialog.CreateAskMessage().ConvertToJson();
                    if (json is not null)
                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    var json = new XFEAskGPTMessage(false, nowDialog.StreamMode, nowDialog.ChatGPTModel.GetModelString(), null, nowDialog.XFEComProtocol, nowDialog.System, nowDialog[^1].GPTMessage.Content).ConvertToJson();
                    if (json is not null)
                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)), WebSocketMessageType.Text, true, CancellationToken.None);
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
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFEDONE]", messageId, GenerateState.End, dialogId));
                            break;
                        }
                        else if (nowReceivedMessage.Contains("[XFERemoteAPIError]"))
                        {
                            if (!isStarted)
                            {
                                XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start, dialogId));
                                xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", true);
                            }
                            else
                            {
                                xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", false);
                            }
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error, dialogId));
                        }
                        else if (nowReceivedMessage == "[XFE]")
                        {
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start, dialogId));
                            isStarted = true;
                            xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", string.Empty, true);
                        }
                        else
                        {
                            xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", nowReceivedMessage, false);
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Continue, dialogId));
                        }
                    }
                    else
                    {
                        if (nowReceivedMessage == "[DONE]")
                        {
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFEDONE]", messageId, GenerateState.End, dialogId));
                            break;
                        }
                        else if (nowReceivedMessage.Contains("[XFERemoteAPIError]"))
                        {
                            if (!isStarted)
                            {
                                XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start, dialogId));
                                xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", true);
                            }
                            else
                            {
                                xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", false);
                            }
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error, dialogId));
                        }
                        else if (nowReceivedMessage == "[XFE]")
                        {
                            XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start, dialogId));
                            isStarted = true;
                            xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", string.Empty, true);
                        }
                        else
                        {
                            if (nowReceivedMessage.Contains("[XFE]"))
                            {
                                nowReceivedMessage = nowReceivedMessage.Replace("[XFE]", string.Empty);
                                xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", nowReceivedMessage, false);
                                XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Continue, dialogId));
                            }
                            else
                            {
                                if (!isStarted)
                                {
                                    XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start, dialogId));
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
            catch (Exception ex) when (ex is not XFEChatGPTException)
            {
                if (!isStarted)
                {
                    XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs("[XFESTART]", messageId, GenerateState.Start, dialogId));
                    xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", true);
                }
                else
                {
                    xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", "该回复发生了一个错误，可能需要重新回复", false);
                }
                XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(ex.ToString(), messageId, GenerateState.Error, dialogId));
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
                if (nowDialog.MemorableMode)
                {
                    var json = nowDialog.CreateAskMessage().ConvertToJson();
                    if (json is not null)
                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    var json = new XFEAskGPTMessage(false, nowDialog.StreamMode, nowDialog.ChatGPTModel.GetModelString(), null, nowDialog.XFEComProtocol, nowDialog.System, nowDialog[^1].GPTMessage.Content).ConvertToJson();
                    if (json is not null)
                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)), WebSocketMessageType.Text, true, CancellationToken.None);
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
                    XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(nowReceivedMessage, messageId, GenerateState.Start, dialogId));
                }
                else
                {
                    xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", nowReceivedMessage, true);
                    XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(nowReceivedMessage.Replace("[XFERemoteAPIError]", string.Empty), messageId, GenerateState.Error, dialogId));
                }
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed.", CancellationToken.None);
            }
            catch (Exception ex)
            {
                xFEGPTMemoryDialog.InstanceUpdate(dialogId, messageId + "_Assistant", ex.ToString(), true);
                XFEChatGPTMessageReceived?.Invoke(this, new PrivateMemorableGPTMessageReceivedEventArgs(ex.ToString(), messageId, GenerateState.Error, dialogId));
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
    public event EventHandler<MemorableGPTMessageReceivedEventArgs>? XFEChatGPTMessageReceived;
    #endregion
    #region 构造方法
    /// <summary>
    /// 创建一个有记忆功能的XFEChatGPT对象
    /// </summary>
    public MemorableXFEChatGPT() => xFEGPTMemoryDialog = new PrivateXFEGPTMemoryDialog();
    #endregion
}