using System.Net.WebSockets;

namespace XFEExtension.NetCore.CyberComm;

record class CyberCommClientEventArgsImpl : CyberCommClientEventArgs
{
    internal CyberCommClientEventArgsImpl(ClientWebSocket clientWebSocket, string message) : base(clientWebSocket, message) { }
    internal CyberCommClientEventArgsImpl(ClientWebSocket clientWebSocket, byte[] bytes) : base(clientWebSocket, bytes) { }
    internal CyberCommClientEventArgsImpl(ClientWebSocket clientWebSocket, XFECyberCommException ex) : base(clientWebSocket, ex) { }
}
