using System.Net.WebSockets;

namespace XFE各类拓展.NetCore.CyberComm.XCCNetWork;

class XCCConnectedEventArgsImpl(XCCGroup group, XCCClientType xCCClientType, ClientWebSocket? textMessageClientWebSocket, ClientWebSocket? fileTransportClientWebSocket) : XCCConnectedEventArgs(group, xCCClientType, textMessageClientWebSocket, fileTransportClientWebSocket) { }
