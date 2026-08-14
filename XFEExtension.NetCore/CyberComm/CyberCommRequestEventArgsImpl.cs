using System.Collections.Specialized;
using System.Net;

namespace XFEExtension.NetCore.CyberComm;

record CyberCommRequestEventArgsImpl : CyberCommRequestEventArgs
{
    /// <summary>
    /// 使用 HTTP 请求、响应及客户端信息创建 CyberComm 服务器请求事件参数。
    /// </summary>
    /// <param name="RequestUrl">请求的 URL 地址。</param>
    /// <param name="RequestMethod">HTTP 请求方法。</param>
    /// <param name="RequestBody">请求体；没有请求体时为 <see langword="null"/>。</param>
    /// <param name="RequestHeaders">请求头集合。</param>
    /// <param name="QueryString">查询字符串参数集合。</param>
    /// <param name="Request">原始 HTTP 请求对象。</param>
    /// <param name="Response">用于回复客户端的 HTTP 响应对象。</param>
    /// <param name="ClientIP">发起请求的客户端 IP 地址。</param>
    public CyberCommRequestEventArgsImpl(Uri? RequestUrl, string RequestMethod, string? RequestBody, NameValueCollection RequestHeaders, NameValueCollection QueryString, HttpListenerRequest Request, HttpListenerResponse Response, string ClientIP) : base(RequestUrl, RequestMethod, RequestBody, RequestHeaders, QueryString, Request, Response, ClientIP)
    {
    }
}
