using System.Net.WebSockets;

namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

class XCCExceptionMessageReceivedEventArgsImpl(XCCGroup group, ClientWebSocket? textMessageClientWebSocket, ClientWebSocket? fileTransportClientWebSocket, XCCClientType xCCClientType, string? sender, string? messageId, XFECyberCommException exception) : XCCExceptionMessageReceivedEventArgs(group, textMessageClientWebSocket, fileTransportClientWebSocket, xCCClientType, sender, messageId, exception) { }