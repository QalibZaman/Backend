using System.Linq;
using System.Net;
using System.Net.Mail;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Infrastructure.Options;

namespace NettoRetail.Infrastructure.Services;

public class EmailSender(
    ILogger<EmailSender> logger, IConfiguration configuration) : IEmailSender
{
    public async Task SendAsync(EmailModel model, Guid userId)
    {
        try
        {
            var connectionString = configuration["AZURE_EMAIL_CONNECTION_STRING"]
                                   ?? throw new InvalidOperationException(
                                       "connection_string is not configured.");

            var senderAddress = configuration["AZURE_EMAIL_SENDER_ADDRESS"]
                                ?? throw new InvalidOperationException(
                                    "sender_address is not configured.");

            var emailClient = new EmailClient(connectionString);

            var emailContent = new EmailContent(model.Subject)
            {
                Html = model.Message
            };

            var emailMessage = new EmailMessage(
                senderAddress,
                model.Email,
                emailContent);

            await emailClient.SendAsync(
                WaitUntil.Completed,
                emailMessage);

            logger.LogInformation(
                "Email successfully sent. User id = {UserId}",
                userId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send email. User id = {UserId}",
                userId);
        }
    }
    public async Task SendBulkAsync(BulkEmailModel model)
    {
        if (model.EmailAddresses == null || !model.EmailAddresses.Any())
            return;

        var connectionString = configuration["connection_string"]
                               ?? throw new InvalidOperationException("connection_string is not configured.");

        var senderAddress = configuration["sender_address"]
                            ?? throw new InvalidOperationException("sender_address is not configured.");

        var emailClient = new EmailClient(connectionString);

        foreach (var email in model.EmailAddresses)
        {
            if (string.IsNullOrWhiteSpace(email))
                continue;

            try
            {
                var emailContent = new EmailContent(model.Subject)
                {
                    Html = model.Message
                };

                var emailMessage = new EmailMessage(
                    senderAddress,
                    email,
                    emailContent);

                await emailClient.SendAsync(
                    WaitUntil.Completed,
                    emailMessage);

                logger.LogInformation(
                    "Email successfully sent. Email = {Email}",
                    email);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to send email. Email = {Email}",
                    email);
            }
        }
    }
}
