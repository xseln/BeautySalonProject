using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace BeautySalonProject.Services
{
    public class EmailSettings
    {
        public string AdminEmail { get; set; } = "";
        public string SmtpHost { get; set; } = "";
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; } = "";
        public string SmtpPass { get; set; } = "";
        public string FromName { get; set; } = "SH Beauty Salon";
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _s;

        public SmtpEmailSender(IOptions<EmailSettings> s) => _s = s.Value;

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            using var msg = new MailMessage
            {
                From = new MailAddress(_s.SmtpUser, _s.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(to);

            using var client = new SmtpClient(_s.SmtpHost, _s.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_s.SmtpUser, _s.SmtpPass)
            };

            await client.SendMailAsync(msg);
        }
    }
}
