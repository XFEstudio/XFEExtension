using System.Net.WebSockets;

namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

class XCCTextMessageReceivedEventArgsImpl(XCCGroup group, ClientWebSocket? textMessageClientWebSocket, ClientWebSocket? fileTransportClientWebSocket, XCCClientType xCCClientType, string messageId, XCCTextMessageType messageType, string message, string sender, DateTime sendTime, bool isHistory) : XCCTextMessageReceivedEventArgs(group, textMessageClientWebSocket, fileTransportClientWebSocket, xCCClientType, messageId, messageType, message, sender, sendTime, isHistory) { }
