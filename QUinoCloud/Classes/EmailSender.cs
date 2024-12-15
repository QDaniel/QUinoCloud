using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using NuGet.Protocol;
using System.Net;
using System.Net.Mail;
using System.Runtime.Intrinsics.X86;

namespace QUinoCloud.Classes
{
    public class EmailSender(IOptions<EmailSettings> settings) : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient
            {
                Port = settings.Value.MailPort > 0 ? settings.Value.MailPort : 25,
                Host = settings.Value.MailServer ?? "localhost", //or another email sender provider
                EnableSsl = settings.Value.UseSSL,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };
            if (!string.IsNullOrWhiteSpace(settings.Value.AuthPwd))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(settings.Value.AuthUser, settings.Value.AuthPwd);
            }
            return client.SendMailAsync(settings.Value.Sender ?? string.Empty, email, subject, htmlMessage);
        }
    }
    public class EmailSettings
    {
        public string? MailServer { get; set; }
        public int MailPort { get; set; }
        public bool UseSSL { get; set; }
        public string? Sender { get; set; }
        public string? AuthUser { get; set; }
        public string? AuthPwd { get; set; }
    }
}

