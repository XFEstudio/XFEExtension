using System.Net.WebSockets;
using XFEExtension.NetCore.Exceptions;

namespace XFEExtension.NetCore.CyberComm;

record CyberCommClientEventArgsImpl : CyberCommClientEventArgs
{
    internal CyberCommClientEventArgsImpl(ClientWebSocket clientWebSocket, string message, bool endOfMessage) : base(clientWebSocket, message, endOfMessage) { }
    internal CyberCommClientEventArgsImpl(ClientWebSocket clientWebSocket, byte[] bytes, bool endOfMessage) : base(clientWebSocket, bytes, endOfMessage) { }
    internal CyberCommClientEventArgsImpl(ClientWebSocket clientWebSocket, XFECyberCommException ex) : base(clientWebSocket, ex) { }
}
