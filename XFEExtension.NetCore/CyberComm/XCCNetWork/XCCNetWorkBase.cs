namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

class XCCNetWorkBase
{
    /// <summary>
    /// 明文消息接收时触发
    /// </summary>
    public EventHandler<XCCTextMessageReceivedEventArgs>? textMessageReceived;
    /// <summary>
    /// 二进制消息接收时触发
    /// </summary>
    public EventHandler<XCCBinaryMessageReceivedEventArgs>? binaryMessageReceived;
    /// <summary>
    /// 异常消息接收时触发
    /// </summary>
    public EventHandler<XCCExceptionMessageReceivedEventArgs>? exceptionMessageReceived;
    /// <summary>
    /// 连接关闭时触发
    /// </summary>
    public EventHandler<XCCConnectionClosedEventArgs>? connectionClosed;
    /// <summary>
    /// 连接成功时触发
    /// </summary>
    public EventHandler<XCCConnectedEventArgs>? connected;
}
