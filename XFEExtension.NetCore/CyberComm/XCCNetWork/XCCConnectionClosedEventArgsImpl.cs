using System.Net.WebSockets;

namespace XFEExtension.NetCore.CyberComm.XCCNetWork;

class XCCConnectionClosedEventArgsImpl(XCCGroup group, XCCClientType xCCClientType, ClientWebSocket? textMessageClientWebSocket, ClientWebSocket? fileTransportClientWebSocket, bool closedNormally) : XCCConnectionClosedEventArgs(group, xCCClientType, textMessageClientWebSocket, fileTransportClientWebSocket, closedNormally) { }
