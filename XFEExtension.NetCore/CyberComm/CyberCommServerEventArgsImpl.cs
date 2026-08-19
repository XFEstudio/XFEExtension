using System.Collections.Specialized;
using System.Net.WebSockets;
using XFEExtension.NetCore.Exceptions;

namespace XFEExtension.NetCore.CyberComm;

record CyberCommServerEventArgsImpl : CyberCommServerEventArgs
{
    internal CyberCommServerEventArgsImpl(Uri? requestUrl, WebSocket webSocket, string message, string ipAddress, NameValueCollection wsHeader, bool endOfMessage, int localPort) : base(requestUrl, webSocket, message, ipAddress, wsHeader, endOfMessage, localPort) { }
    internal CyberCommServerEventArgsImpl(Uri? requestUrl, WebSocket webSocket, byte[] bytes, string ipAddress, NameValueCollection wsHeader, bool endOfMessage, int localPort) : base(requestUrl, webSocket, bytes, ipAddress, wsHeader, endOfMessage, localPort) { }
    internal CyberCommServerEventArgsImpl(Uri? requestUrl, WebSocket webSocket, XFECyberCommException ex, string ipAddress, NameValueCollection wsHeader, int localPort) : base(requestUrl, webSocket, ex, ipAddress, wsHeader, localPort) { }
}
