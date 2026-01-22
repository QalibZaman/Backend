using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Infrastructure.Options;

namespace NettoRetail.Infrastructure.Services;

public class EmailSender(IOptions<EmailOptions> emailOptions,
    ILogger<EmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailModel model)
    {
        logger.LogInformation("Building SMTP client...");

        using var client = new SmtpClient(
            emailOptions.Value.Host,
            emailOptions.Value.Port)
        {
            EnableSsl = emailOptions.Value.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                emailOptions.Value.Username,
                emailOptions.Value.Password)
        };

        logger.LogInformation("Building message model...");

        using var message = new MailMessage
        {
            From = new MailAddress(emailOptions.Value.Sender, emailOptions.Value.Username),
            Subject = model.Subject,
            Body = model.Message,
            IsBodyHtml = true
        };

        message.To.Add(model.Email);

        logger.LogInformation("Sending Email...");

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email");
        }

        logger.LogInformation("Email succesfully sent!");
    }

    public async Task SendBulkAsync(BulkEmailModel model)
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

        foreach (var email in model.EmailAddresses)
        {
            message.To.Add(email);

            await client.SendMailAsync(message);
        }
    }
}
