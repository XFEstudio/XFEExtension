using System.Net.WebSockets;

namespace XFE各类拓展.NetCore.CyberComm.XCCNetWork;

/// <summary>
/// XCC网络通讯期间异常事件
/// </summary>
public abstract class XCCExceptionMessageReceivedEventArgs : XCCMessageReceivedEventArgs
{
    /// <summary>
    /// 异常信息
    /// </summary>
    public XFECyberCommException Exception { get; }
    internal XCCExceptionMessageReceivedEventArgs(XCCGroup group, ClientWebSocket? textMessageClientWebSocket, ClientWebSocket? fileTransportClientWebSocket, XCCClientType xCCClientType, string? sender, string? messageId, XFECyberCommException exception) : base(group, textMessageClientWebSocket, fileTransportClientWebSocket, xCCClientType, sender, messageId) { Exception = exception; }
}
