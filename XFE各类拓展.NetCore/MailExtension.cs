using System;
using System.Net;
using System.Net.Mail;

namespace XFE各类拓展.MailExtension
{
    /// <summary>
    /// XFE邮件的拓展
    /// </summary>
    public static class MailExtension
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="Content">内容</param>
        /// <param name="Password">密码</param>
        /// <param name="SubTitle">标题</param>
        /// <param name="To">目标邮箱地址</param>
        /// <exception cref="XFEMailException"></exception>
        public static void SendEmail(this string Content, string Password, string SubTitle, string To)
        {
            try
            {
                using (var client = new SmtpClient("smtp.exmail.qq.com", 587))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("mail@xfegzs.com", Password);
                    client.EnableSsl = true;
                    MailAddress fromMailAddress = new MailAddress("mail@xfegzs.com", "XFEMail");
                    MailMessage message = new MailMessage();
                    message.From = fromMailAddress;
                    message.To.Add(To);
                    message.Subject = SubTitle;
                    message.Body = Content;
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
        /// <param name="Password">密码</param>
        /// <param name="Name">名称</param>
        /// <param name="SubTitle">标题</param>
        /// <param name="Content">内容</param>
        /// <param name="To">目标邮箱地址</param>
        /// <exception cref="XFEMailException"></exception>
        public static void SendEmail(this string Content, string Password, string Name, string SubTitle, string To)
        {
            try
            {
                using (var client = new SmtpClient("smtp.exmail.qq.com", 587))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("mail@xfegzs.com", Password);
                    client.EnableSsl = true;
                    MailAddress fromMailAddress = new MailAddress("mail@xfegzs.com", Name);
                    MailMessage message = new MailMessage();
                    message.From = fromMailAddress;
                    message.To.Add(To);
                    message.Subject = SubTitle;
                    message.Body = Content;
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
    /// <summary>
    /// XFE邮件
    /// </summary>
    public class XFEMail
    {
        /// <summary>
        /// 发送者名称
        /// </summary>
        public string SenderName { get; set; }
        /// <summary>
        /// 对方邮箱地址
        /// </summary>
        public string ToAddress { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="SubTitle">标题</param>
        /// <param name="Content">内容</param>
        /// <exception cref="XFEMailException"></exception>
        public void SendEmail(string SubTitle, string Content)
        {
            try
            {
                using (var client = new SmtpClient("smtp.exmail.qq.com", 587))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("mail@xfegzs.com", Password);
                    client.EnableSsl = true;
                    MailAddress fromMailAddress = new MailAddress("mail@xfegzs.com", SenderName);
                    MailMessage message = new MailMessage();
                    message.From = fromMailAddress;
                    message.To.Add(ToAddress);
                    message.Subject = SubTitle;
                    message.Body = Content;
                    // 发送邮件
                    client.SendAsync(message, null);
                }
            }
            catch (Exception ex)
            {
                throw new XFEMailException("邮件发送错误！" + ex.Message);
            }
        }
        /// <summary>
        /// XFE邮件系统
        /// </summary>
        /// <param name="senderName">名称</param>
        /// <param name="toAddress">目标地址</param>
        /// <param name="password">密码</param>
        public XFEMail(string senderName, string toAddress, string password)
        {
            SenderName = senderName;
            ToAddress = toAddress;
            Password = password;
        }
        /// <summary>
        /// XFE邮件系统
        /// </summary>
        /// <param name="password">密码</param>
        public XFEMail(string password)
        {
            Password = password;
        }
        /// <summary>
        /// XFE邮件系统
        /// </summary>
        public XFEMail() { }
    }
}
