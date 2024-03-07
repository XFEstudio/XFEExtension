using System.Collections;
using XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;

namespace XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

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
            if (xFEGPTMessages.Count > 0 && xFEGPTMessages[0].GPTMessage.Role == "system")
            {
                return xFEGPTMessages[0].GPTMessage.Content;
            }
            else
            {
                throw new XFEChatGPTException("消息集合中没有系统消息！");
            }
        }
        set
        {
            if (xFEGPTMessages.Count > 0 && xFEGPTMessages[0].GPTMessage.Role == "system")
            {
                xFEGPTMessages[0].GPTMessage.Content = value;
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
            if (xFEGPTMessages[i].MessageId == messageId)
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
    public XFEGPTMessage? GetXFEGPTMessageByMessageId(string messageId)
    {
        foreach (XFEGPTMessage xFEGPTMessage in xFEGPTMessages)
        {
            if (xFEGPTMessage.MessageId == messageId)
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
        List<GPTMessage> gPTMessages = [];
        foreach (XFEGPTMessage xFEGPTMessage in xFEGPTMessages)
        {
            gPTMessages.Add(xFEGPTMessage.GPTMessage);
        }
        return [.. gPTMessages];
    }
    /// <summary>
    /// 以字符串的形式获取本集合的GPTMessage数组
    /// </summary>
    /// <returns></returns>
    public string[] GetGPTMessageStrings()
    {
        List<string> gPTMessages = [];
        foreach (XFEGPTMessage xFEGPTMessage in xFEGPTMessages)
        {
            gPTMessages.Add(xFEGPTMessage.GPTMessage.Content);
        }
        return [.. gPTMessages];
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
        return xFEGPTMessages[Count - 1].GPTMessage.Role;
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
        xFEGPTMessages = [];
    }
    #endregion
}