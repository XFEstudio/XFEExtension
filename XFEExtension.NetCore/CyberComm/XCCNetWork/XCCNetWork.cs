namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

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
        var group = new XCCGroupImpl(Guid.NewGuid().ToString(), groupId, sender, xCCNetWorkBase);
        Groups.Add(group);
        return group;
    }
    /// <summary>
    /// XCC网络通信会话
    /// </summary>
    public XCCNetWork()
    {
        xCCNetWorkBase = new XCCNetWorkBase();
        Groups = [];
    }
}