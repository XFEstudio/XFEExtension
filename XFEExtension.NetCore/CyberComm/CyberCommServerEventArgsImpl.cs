using System.Collections.Specialized;
using System.Net.WebSockets;
using XFEExtension.NetCore.Exceptions;

namespace XFEExtension.NetCore.CyberComm;

record CyberCommServerEventArgsImpl : CyberCommServerEventArgs
{
    internal CyberCommServerEventArgsImpl(Uri? requestUrl, WebSocket webSocket, string message, string ipAddress, NameValueCollection wsHeader, bool endOfMessage) : base(requestUrl, webSocket, message, ipAddress, wsHeader, endOfMessage) { }
    internal CyberCommServerEventArgsImpl(Uri? requestUrl, WebSocket webSocket, byte[] bytes, string ipAddress, NameValueCollection wsHeader, bool endOfMessage) : base(requestUrl, webSocket, bytes, ipAddress, wsHeader, endOfMessage) { }
    internal CyberCommServerEventArgsImpl(Uri? requestUrl, WebSocket webSocket, XFECyberCommException ex, string ipAddress, NameValueCollection wsHeader) : base(requestUrl, webSocket, ex, ipAddress, wsHeader) { }
}