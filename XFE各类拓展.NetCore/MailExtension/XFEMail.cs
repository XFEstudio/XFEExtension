using System.Net;
using System.Net.Mail;

namespace XFE各类拓展.NetCore.MailExtension
{
    /// <summary>
    /// XFE邮件
    /// </summary>
    public class XFEMail
    {
        /// <summary>
        /// 发送者名称
        /// </summary>
        public string? SenderName { get; set; }
        /// <summary>
        /// 对方邮箱地址
        /// </summary>
        public string? ToAddress { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string? Password { get; set; }
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="subTitle">标题</param>
        /// <param name="content">内容</param>
        /// <exception cref="XFEMailException"></exception>
        public void SendEmail(string subTitle, string content)
        {
            try
            {
                using var client = new SmtpClient("smtp.exmail.qq.com", 587);
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("mail@xfegzs.com", Password);
                client.EnableSsl = true;
                MailAddress fromMailAddress = new MailAddress("mail@xfegzs.com", SenderName);
                MailMessage message = new MailMessage();
                message.From = fromMailAddress;
                if (ToAddress is not null)
                    message.To.Add(ToAddress);
                else
                    throw new XFEMailException("地址不能为空！");
                message.Subject = subTitle;
                message.Body = content;
                // 发送邮件
                client.SendAsync(message, null);
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
