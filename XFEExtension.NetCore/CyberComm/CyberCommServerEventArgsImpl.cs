using System.Collections.Specialized;
using System.Net.WebSockets;

namespace XFEExtension.NetCore.CyberComm;

record class CyberCommServerEventArgsImpl : CyberCommServerEventArgs
{
    internal CyberCommServerEventArgsImpl(Uri? requestURL, WebSocket webSocket, string message, string ipAddress, NameValueCollection wsHeader) : base(requestURL, webSocket, message, ipAddress, wsHeader) { }
    internal CyberCommServerEventArgsImpl(Uri? requestURL, WebSocket webSocket, byte[] bytes, string ipAddress, NameValueCollection wsHeader) : base(requestURL, webSocket, bytes, ipAddress, wsHeader) { }
    internal CyberCommServerEventArgsImpl(Uri? requestURL, WebSocket webSocket, XFECyberCommException ex, string ipAddress, NameValueCollection wsHeader) : base(requestURL, webSocket, ex, ipAddress, wsHeader) { }
}