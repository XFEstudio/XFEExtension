using System.Net;
using System.Net.Mail;

namespace XFEExtension.NetCore.MailExtension;

/// <summary>
/// XFE邮件的拓展
/// </summary>
public static class MailExtension
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="email">邮箱</param>
    /// <param name="host">邮箱主机</param>
    /// <param name="displayName">显示名称</param>
    /// <param name="password">密码</param>
    /// <param name="subTitle">标题</param>
    /// <param name="to">目标邮箱地址</param>
    /// <param name="port">端口</param>
    /// <exception cref="XFEMailException"></exception>
    public static void SendEmail(this string content, string email, string host, string password, string displayName, string subTitle, string to, int port = 587)
    {
        try
        {
            using var client = new SmtpClient(host, port);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(email, password);
            client.EnableSsl = true;
            MailAddress fromMailAddress = new(email, displayName);
            MailMessage message = new()
            {
                From = fromMailAddress
            };
            message.To.Add(to);
            message.Subject = subTitle;
            message.Body = content;
            client.Send(message);
        }
        catch (Exception ex)
        {
            throw new XFEMailException("邮件发送错误！" + ex.Message);
        }
    }
}
