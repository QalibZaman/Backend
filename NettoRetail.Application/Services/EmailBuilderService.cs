using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Infrastructure.EmailTemplates;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Utilities;
using System.Security.Claims;

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

    public async Task<EmailModel> BuildAccountDeletedEmailAsync(string email, string firstName)
    {
        var templateModel = new { first_name = firstName, current_year = DateTime.UtcNow.Year };

        var body = await emailTemplateService.RenderEmailTemplateAsync(
            EmailTemplates.AccountDeleted,
            templateModel);

        return new EmailModel
        {
            Email = email,
            Subject = EmailSubjects.AccountDeleted,
            Message = body
        };
    }

    public async Task<EmailModel> BuildFailedDeletionAttemptsEmailAsync(string email, string firstName)
    {
        var templateModel = new { first_name = firstName, current_year = DateTime.UtcNow.Year };

        var body = await emailTemplateService.RenderEmailTemplateAsync(
            EmailTemplates.FailedDeletionAttempts,
            templateModel);

        return new EmailModel
        {
            Email = email,
            Subject = EmailSubjects.FailedDeletionAttempts,
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

    public Task<EmailModel> BuildResetPasswordEmailAsync(string email, string firstName)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(AuthClaims.TokenType, AuthTokenTypes.Reset)
        };

        var resetToken = jwtManager.Generate(claims, TimeSpan.FromHours(passwordOptions.Value.TokenExpirationHours));
        var resetLink = string.Format(passwordOptions.Value.ResetLink, Uri.EscapeDataString(resetToken));

        var emailModel = new EmailModel
        {
            Email = email,
            Subject = "Reset your password",
            Message = $"Hello, {firstName}. Reset your password here: {resetLink}"
        };

        return Task.FromResult(emailModel);
    }
}