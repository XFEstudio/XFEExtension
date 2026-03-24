namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

class XCCNetWorkBase
{
    /// <summary>
    /// 明文消息接收时触发
    /// </summary>
    public EventHandler<XCCTextMessageReceivedEventArgs>? TextMessageReceived;
    /// <summary>
    /// 二进制消息接收时触发
    /// </summary>
    public EventHandler<XCCBinaryMessageReceivedEventArgs>? BinaryMessageReceived;
    /// <summary>
    /// 异常消息接收时触发
    /// </summary>
    public EventHandler<XCCExceptionMessageReceivedEventArgs>? ExceptionMessageReceived;
    /// <summary>
    /// 连接关闭时触发
    /// </summary>
    public EventHandler<XCCConnectionClosedEventArgs>? ConnectionClosed;
    /// <summary>
    /// 连接成功时触发
    /// </summary>
    public EventHandler<XCCConnectedEventArgs>? Connected;
}
