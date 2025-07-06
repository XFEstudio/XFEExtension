using System.Net.WebSockets;

namespace XFEExtension.NetCore.CyberComm;

record class CyberCommClientEventArgsImpl : CyberCommClientEventArgs
{
    internal CyberCommClientEventArgsImpl(ClientWebSocket clientWebSocket, string message, bool endOfMessage) : base(clientWebSocket, message, endOfMessage) { }
    internal CyberCommClientEventArgsImpl(ClientWebSocket clientWebSocket, byte[] bytes, bool endOfMessage) : base(clientWebSocket, bytes, endOfMessage) { }
    internal CyberCommClientEventArgsImpl(ClientWebSocket clientWebSocket, XFECyberCommException ex) : base(clientWebSocket, ex) { }
}
