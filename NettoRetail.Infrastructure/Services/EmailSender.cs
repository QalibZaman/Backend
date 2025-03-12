using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Infrastructure.Options;

namespace NettoRetail.Infrastructure.Services;

public class EmailSender(IOptions<EmailOptions> emailOptions) : IEmailSender
{
    public async Task SendAsync(EmailModel model)
    {
        using var client = new SmtpClient(
            emailOptions.Value.Host,
            emailOptions.Value.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(
                emailOptions.Value.Sender,
                emailOptions.Value.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(emailOptions.Value.Sender),
            Subject = model.Subject,
            Body = model.Message,
            IsBodyHtml = true
        };

        message.To.Add(model.Email);

        await client.SendMailAsync(message);
    }
}
