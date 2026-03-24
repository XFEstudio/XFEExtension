namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// XCC网络通讯
/// </summary>
public class XCCNetWork
{
    private readonly XCCNetWorkBase _xCCNetWorkBase;
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
            _xCCNetWorkBase.TextMessageReceived += value;
        }
        remove
        {
            _xCCNetWorkBase.TextMessageReceived -= value;
        }
    }
    /// <summary>
    /// 二进制消息接收时触发
    /// </summary>
    public event EventHandler<XCCBinaryMessageReceivedEventArgs> BinaryMessageReceived
    {
        add
        {
            _xCCNetWorkBase.BinaryMessageReceived += value;
        }
        remove
        {
            _xCCNetWorkBase.BinaryMessageReceived -= value;
        }
    }
    /// <summary>
    /// 异常消息接收时触发
    /// </summary>
    public event EventHandler<XCCExceptionMessageReceivedEventArgs> ExceptionMessageReceived
    {
        add
        {
            _xCCNetWorkBase.ExceptionMessageReceived += value;
        }
        remove
        {
            _xCCNetWorkBase.ExceptionMessageReceived -= value;
        }
    }
    /// <summary>
    /// 连接关闭时触发
    /// </summary>
    public event EventHandler<XCCConnectionClosedEventArgs> ConnectionClosed
    {
        add
        {
            _xCCNetWorkBase.ConnectionClosed += value;
        }
        remove
        {
            _xCCNetWorkBase.ConnectionClosed -= value;
        }
    }
    /// <summary>
    /// 连接成功时触发
    /// </summary>
    public event EventHandler<XCCConnectedEventArgs> Connected
    {
        add
        {
            _xCCNetWorkBase.Connected += value;
        }
        remove
        {
            _xCCNetWorkBase.Connected -= value;
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
        var group = new XCCGroupImpl(Guid.NewGuid().ToString(), groupId, sender, _xCCNetWorkBase);
        Groups.Add(group);
        return group;
    }
    /// <summary>
    /// XCC网络通信会话
    /// </summary>
    public XCCNetWork()
    {
        _xCCNetWorkBase = new XCCNetWorkBase();
        Groups = [];
    }
}