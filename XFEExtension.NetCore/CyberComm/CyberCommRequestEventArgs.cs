using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace XFEExtension.NetCore.CyberComm;

/// <summary>
/// CyberComm HTTP 请求事件参数。新代码应优先使用 <see cref="CyberCommHttpRequestContext"/>。
/// </summary>
public abstract record CyberCommRequestEventArgs
{
    private readonly HttpListenerRequest? _request;
    private readonly HttpListenerResponse? _response;

    protected CyberCommRequestEventArgs(CyberCommHttpRequestContext context)
    {
        Context = context;
        RequestUrl = context.RequestUri;
        RequestMethod = context.Method;
        RequestBody = context.Body.IsEmpty ? null : context.RequestBody;
        RequestHeaders = ToNameValueCollection(context.Headers);
        QueryString = ToNameValueCollection(context.Query);
        ClientIP = context.ClientIp;
    }

    protected CyberCommRequestEventArgs(Uri? requestUrl, string requestMethod, string? requestBody,
        NameValueCollection requestHeaders, NameValueCollection queryString, HttpListenerRequest request,
        HttpListenerResponse response, string clientIP)
    {
        RequestUrl = requestUrl;
        RequestMethod = requestMethod;
        RequestBody = requestBody;
        RequestHeaders = requestHeaders;
        QueryString = queryString;
        _request = request;
        _response = response;
        ClientIP = clientIP;
    }

    public CyberCommHttpRequestContext? Context { get; }
    public Uri? RequestUrl { get; }
    public string RequestMethod { get; }
    public string? RequestBody { get; }
    public NameValueCollection RequestHeaders { get; }
    public NameValueCollection QueryString { get; }

    /// <summary>旧 HttpListener 请求。Socket 传输模式下为 null。</summary>
    [Obsolete("请使用 Context 和与监听器无关的请求属性")]
    public HttpListenerRequest? Request => _request;

    /// <summary>旧 HttpListener 响应。Socket 传输模式下为 null。</summary>
    [Obsolete("请使用 Context.Response 或 ReplyAndClose")]
    public HttpListenerResponse? Response => _response;

    public string ClientIP { get; }
    /// <summary>贯穿请求、响应和日志的关联标识。</summary>
    public string CorrelationId => Context?.CorrelationId ?? string.Empty;

    /// <summary>
    /// 从跨平台 HTTP 上下文创建兼容事件参数。
    /// </summary>
    public static CyberCommRequestEventArgs FromContext(CyberCommHttpRequestContext context)
        => new CyberCommRequestEventArgsImpl(context);

    public async Task ReplyAndClose(string message, HttpStatusCode statusCode = HttpStatusCode.OK)
        => await ReplyAndClose(message, statusCode, "text/plain; charset=utf-8").ConfigureAwait(false);

    /// <summary>写入带内容类型的响应并结束请求。</summary>
    public async Task ReplyAndClose(string message, HttpStatusCode statusCode, string contentType)
    {
        if (Context is not null)
        {
            await Context.Response.WriteTextAsync(message, statusCode, contentType).ConfigureAwait(false);
            Context.Response.Complete();
            return;
        }
        var response = _response ?? throw new InvalidOperationException("当前请求没有响应对象");
        response.StatusCode = (int)statusCode;
        response.ContentType = contentType;
        var buffer = Encoding.UTF8.GetBytes(message);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer).ConfigureAwait(false);
        response.Close();
    }

    public async Task ReplyMessage(string message)
    {
        if (Context is not null)
        {
            await Context.Response.WriteTextAsync(message).ConfigureAwait(false);
            return;
        }
        var response = _response ?? throw new InvalidOperationException("当前请求没有响应对象");
        var buffer = Encoding.UTF8.GetBytes(message);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer).ConfigureAwait(false);
    }

    public async Task ReplyBinaryMessage(byte[] bytes)
    {
        if (Context is not null)
        {
            await Context.Response.WriteAsync(bytes).ConfigureAwait(false);
            return;
        }
        var response = _response ?? throw new InvalidOperationException("当前请求没有响应对象");
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
    }

    public void Close(HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        if (Context is not null)
        {
            Context.Response.Complete(statusCode);
            return;
        }
        var response = _response ?? throw new InvalidOperationException("当前请求没有响应对象");
        response.StatusCode = (int)statusCode;
        response.Close();
    }

    public void CloseWhitBadRequest() => Close(HttpStatusCode.BadRequest);

    private static NameValueCollection ToNameValueCollection(IReadOnlyDictionary<string, IReadOnlyList<string>> source)
    {
        var result = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, values) in source)
            foreach (var value in values)
                result.Add(name, value);
        return result;
    }
}
