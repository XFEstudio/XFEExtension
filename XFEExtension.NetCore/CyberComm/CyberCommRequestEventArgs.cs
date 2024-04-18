using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace XFEExtension.NetCore.CyberComm;

/// <summary>
/// CyberComm服务器Http请求事件参数
/// </summary>
/// <param name="RequestURL">请求的URL地址</param>
/// <param name="RequestMethod">请求方法</param>
/// <param name="RequestBody">当请求方法为POST时的请求体</param>
/// <param name="RequestHeaders">请求头</param>
/// <param name="QueryString">查询请求</param>
/// <param name="Request">请求对象</param>
/// <param name="Response">响应对象</param>
/// <param name="ClientIP">客户端IP</param>
public abstract record class CyberCommRequestEventArgs(Uri? RequestURL, string RequestMethod, string? RequestBody, NameValueCollection RequestHeaders, NameValueCollection QueryString, HttpListenerRequest Request, HttpListenerResponse Response, string ClientIP)
{
    /// <summary>
    /// 回复消息并关闭连接
    /// </summary>
    /// <param name="message">待回复消息</param>
    /// <param name="statusCode">状态码</param>
    /// <returns></returns>
    public async Task ReplyAndClose(string message, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Response.StatusCode = (int)statusCode;
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        Response.ContentLength64 = buffer.Length;
        await Response.OutputStream.WriteAsync(buffer);
        Response.Close();
    }
    /// <summary>
    /// 回复消息
    /// </summary>
    /// <param name="message">待回复消息</param>
    /// <returns></returns>
    public async Task ReplyMessage(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        Response.ContentLength64 = buffer.Length;
        await Response.OutputStream.WriteAsync(buffer);
    }
    /// <summary>
    /// 回复二进制消息
    /// </summary>
    /// <param name="bytes">二进制消息</param>
    /// <returns></returns>
    public async Task ReplyBinaryMessage(byte[] bytes)
    {
        Response.ContentLength64 = bytes.Length;
        await Response.OutputStream.WriteAsync(bytes);
    }
    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="statusCode">状态码</param>
    public void Close(HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Response.StatusCode = (int)statusCode;
        Response.Close();
    }
    /// <summary>
    /// 以错误请求关闭连接
    /// </summary>
    public void CloseWhitBadRequest()
    {
        Response.StatusCode = 400;
        Response.Close();
    }
}
