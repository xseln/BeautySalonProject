using System.Net;
using System.Net.Mail;

public class EmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtp = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential("shbeautystudioo@gmail.com", "viyayhecqfvsxedy"),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress("shbeautystudioo@gmail.com", "SH Beauty Studio"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        await smtp.SendMailAsync(message);
    }
}
