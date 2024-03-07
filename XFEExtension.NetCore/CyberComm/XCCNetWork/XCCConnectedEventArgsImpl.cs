using System.Net.WebSockets;

namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

class XCCConnectedEventArgsImpl(XCCGroup group, XCCClientType xCCClientType, ClientWebSocket? textMessageClientWebSocket, ClientWebSocket? fileTransportClientWebSocket) : XCCConnectedEventArgs(group, xCCClientType, textMessageClientWebSocket, fileTransportClientWebSocket) { }
