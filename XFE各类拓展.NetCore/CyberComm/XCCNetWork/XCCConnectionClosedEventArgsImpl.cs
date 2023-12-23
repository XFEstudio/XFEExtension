using System.Net.WebSockets;

namespace XFE各类拓展.NetCore.CyberComm.XCCNetWork;

class XCCConnectionClosedEventArgsImpl(XCCGroup group, XCCClientType xCCClientType, ClientWebSocket? textMessageClientWebSocket, ClientWebSocket? fileTransportClientWebSocket, bool closedNormally) : XCCConnectionClosedEventArgs(group, xCCClientType, textMessageClientWebSocket, fileTransportClientWebSocket, closedNormally) { }
