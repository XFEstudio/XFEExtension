using System.Collections;
using XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.DefaultClass;

namespace XFEExtension.NetCore.XFEChatGPT.ChatGPTInnerClass.HelperClass;

/// <summary>
/// GPT对话记录
/// </summary>
public abstract class XFEGPTMemoryDialog : IEnumerable<XFEGPTMessage>
{
    private readonly Dictionary<string, XFEGPTMessageCollection> _gPTMessageWithId;
    internal XFEGPTMemoryDialog()
    {
        _gPTMessageWithId = [];
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
                return _gPTMessageWithId[dialogId];
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
                _gPTMessageWithId[dialogId] = value;
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
        _gPTMessageWithId.Add(dialogId, xFEGPTMessages);
    }
    /// <summary>
    /// 添加多个对话记录
    /// </summary>
    /// <param name="messages">ID和XFEGPT的对话的键值对</param>
    public void AddRange(KeyValuePair<string, XFEGPTMessageCollection>[] messages)
    {
        foreach (var item in messages)
        {
            _gPTMessageWithId.Add(item.Key, item.Value);
        }
    }
    /// <summary>
    /// 移除一个对话记录
    /// </summary>
    /// <param name="dialogId">对话ID</param>
    public void Remove(string dialogId)
    {
        _gPTMessageWithId.Remove(dialogId);
    }
    /// <summary>
    /// 移除多个对话记录
    /// </summary>
    /// <param name="ids">对话ID的数组</param>
    public void RemoveRange(string[] ids)
    {
        foreach (var item in ids)
        {
            _gPTMessageWithId.Remove(item);
        }
    }
    /// <summary>
    /// 清空所有对话的对话记录
    /// </summary>
    public void Clear()
    {
        _gPTMessageWithId.Clear();
    }
    /// <summary>
    /// 更新指定对话ID的对话记录
    /// </summary>
    /// <param name="dialogId">对话ID</param>
    /// <param name="messages">XFEGPT的对话</param>
    public void Update(string dialogId, XFEGPTMessageCollection messages)
    {
        _gPTMessageWithId[dialogId] = messages;
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
        var xFEGPTMessages = _gPTMessageWithId[dialogId];
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

            if (clear)
            {
                xFEGPTMessages.GetXFEGPTMessageByMessageId(messageId)!.GPTMessage.Content = nowMessage;
            }
            else
            {
                xFEGPTMessages.GetXFEGPTMessageByMessageId(messageId)!.GPTMessage.Content += nowMessage;
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
        return _gPTMessageWithId.ContainsKey(dialogId);
    }
    /// <summary>
    /// 获取对话记录
    /// </summary>
    /// <param name="dialogId">对话ID</param>
    /// <returns></returns>
    public XFEGPTMessageCollection GetXFEGPTMessages(string dialogId)
    {
        return _gPTMessageWithId[dialogId];
    }
    /// <summary>
    /// 获取迭代器
    /// </summary>
    /// <returns></returns>
    public IEnumerator<XFEGPTMessage> GetEnumerator()
    {
        foreach (var item in _gPTMessageWithId)
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