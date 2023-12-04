using System.Net;
using System.Net.Mail;

namespace XFE各类拓展.NetCore.MailExtension;

/// <summary>
/// XFE邮件的拓展
/// </summary>
public static class MailExtension
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="password">密码</param>
    /// <param name="subTitle">标题</param>
    /// <param name="to">目标邮箱地址</param>
    /// <exception cref="XFEMailException"></exception>
    public static void SendEmail(this string content, string password, string subTitle, string to)
    {
        try
        {
            using (var client = new SmtpClient("smtp.exmail.qq.com", 587))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("mail@xfegzs.com", password);
                client.EnableSsl = true;
                MailAddress fromMailAddress = new("mail@xfegzs.com", "XFEMail");
                MailMessage message = new()
                {
                    From = fromMailAddress
                };
                message.To.Add(to);
                message.Subject = subTitle;
                message.Body = content;
                // 发送邮件
                client.Send(message);
            }
        }
        catch (Exception ex)
        {
            throw new XFEMailException("邮件发送错误！" + ex.Message);
        }
    }
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="password">密码</param>
    /// <param name="name">名称</param>
    /// <param name="subTitle">标题</param>
    /// <param name="content">内容</param>
    /// <param name="to">目标邮箱地址</param>
    /// <exception cref="XFEMailException"></exception>
    public static void SendEmail(this string content, string password, string name, string subTitle, string to)
    {
        try
        {
            using (var client = new SmtpClient("smtp.exmail.qq.com", 587))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("mail@xfegzs.com", password);
                client.EnableSsl = true;
                MailAddress fromMailAddress = new("mail@xfegzs.com", name);
                MailMessage message = new()
                {
                    From = fromMailAddress
                };
                message.To.Add(to);
                message.Subject = subTitle;
                message.Body = content;
                // 发送邮件
                client.SendAsync(message, null);
            }
        }
        catch (Exception ex)
        {
            throw new XFEMailException("邮件发送错误！" + ex.Message);
        }
    }
}
