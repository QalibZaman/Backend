using System.Net;
using System.Net.Mail;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Infrastructure.Options;

namespace NettoRetail.Infrastructure.Services;

public class EmailSender(IOptions<EmailOptions> emailOptions,
    ILogger<EmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailModel model, Guid userId)
    {
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

        using var message = new MailMessage
        {
            From = new MailAddress(emailOptions.Value.Sender, emailOptions.Value.Username),
            Subject = model.Subject,
            Body = model.Message,
            IsBodyHtml = true
        };

        message.To.Add(model.Email);

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to send email. User id = {userId}");
        }

        logger.LogInformation($"Email succesfully sent! User id = {userId}");
    }

    public async Task SendBulkAsync(BulkEmailModel model)
    {
        if (model.EmailAddresses == null || !model.EmailAddresses.Any())
            return;

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
        foreach (var email in model.EmailAddresses)
        {
            if (string.IsNullOrWhiteSpace(email))
                continue;

            using var message = new MailMessage
            {
                From = new MailAddress(emailOptions.Value.Sender, emailOptions.Value.Username),
                Subject = model.Subject,
                Body = model.Message,
                IsBodyHtml = true
            };

            message.To.Add(email);

            try
            {
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to send email. Email = {email}");
            }
        }
    }
}
