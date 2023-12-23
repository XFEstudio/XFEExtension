using System.Net.WebSockets;

namespace XFE各类拓展.NetCore.CyberComm.XCCNetWork;

class XCCBinaryMessageReceivedEventArgsImpl(XCCGroup group, ClientWebSocket? textMessageClientWebSocket, ClientWebSocket? fileTransportClientWebSocket, XCCClientType xCCClientType, string sender, string messageId, byte[] buffer, XCCBinaryMessageType messageType, string signature, DateTime sendTime, bool isHistory) : XCCBinaryMessageReceivedEventArgs(group, textMessageClientWebSocket, fileTransportClientWebSocket, xCCClientType, sender, messageId, buffer, messageType, signature, sendTime, isHistory) { }
