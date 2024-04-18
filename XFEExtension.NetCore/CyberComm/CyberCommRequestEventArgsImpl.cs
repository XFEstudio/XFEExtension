using System.Collections.Specialized;
using System.Net;

namespace XFEExtension.NetCore.CyberComm;

record class CyberCommRequestEventArgsImpl : CyberCommRequestEventArgs
{
    public CyberCommRequestEventArgsImpl(Uri? RequestURL, string RequestMethod, string? RequestBody, NameValueCollection RequestHeaders, NameValueCollection QueryString, HttpListenerRequest Request, HttpListenerResponse Response, string ClientIP) : base(RequestURL, RequestMethod, RequestBody, RequestHeaders, QueryString, Request, Response, ClientIP)
    {
    }
}