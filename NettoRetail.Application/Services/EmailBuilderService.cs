using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Infrastructure.EmailTemplates;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.Services;

public class EmailBuilderService(
    IJwtManager jwtManager,
    IEmailTemplateService emailTemplateService,
    IOptions<ResetPasswordOptions> passwordOptions,
    ILinkService linkService) : IEmailBuilderService
{
    public async Task<EmailModel> BuildActivateLockedAccountEmailAsync(string email, string firstName, string activationLink)
    {
        var templateModel = new
        {
            first_name = firstName,
            activation_link = activationLink,
            current_year = DateTime.UtcNow.Year,
        };

        var body = await emailTemplateService.RenderEmailTemplateAsync(
            EmailTemplates.ActivateLockedAccount,
            templateModel);

        return new EmailModel
        {
            Email = email,
            Subject = EmailSubjects.AccountUnlock,
            Message = body
        };
    }

    public async Task<EmailModel> BuildNewsletterSubscribedEmailAsync(string email, string fullName, Guid accountId)
    {
        var url = linkService.BuildNewsletterUnsubscribeUrl(accountId);
        var model = new { full_name = fullName, unsubscribe_url = url };
        var emailBody = await emailTemplateService.RenderEmailTemplateAsync(
            EmailTemplates.SubscriptionConfirmation, model);

        return new EmailModel
        {
            Email = email,
            Subject = EmailSubjects.SubscriptionConfirmation,
            Message = emailBody
        };
    }

    public async Task<EmailModel> BuildNewsletterUnsubscribedEmailAsync(string email, string fullName, int year)
    {
        var model = new { full_name = fullName, current_year = year };
        var emailBody =
            await emailTemplateService.RenderEmailTemplateAsync(EmailTemplates.UnsubscriptionConfirmation, model);
        return new EmailModel
        {
            Email = email,
            Subject = EmailSubjects.UnsubscriptionConfirmation,
            Message = emailBody
        };
    }
}