using System.Security.Claims;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Infrastructure.EmailTemplates;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.Services;
public class EmailBuilderService(
    IJwtManager jwtManager,
    IEmailTemplateService emailTemplateService,
    IOptions<ResetPasswordOptions> passwordOptions) : IEmailBuilderService
{
    public async Task<EmailModel> BuildResetPasswordEmailAsync(string email, string firstName)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(AuthClaims.TokenType, AuthTokenTypes.Reset)
        };

        var resetToken = jwtManager.Generate(claims, TimeSpan.FromHours(passwordOptions.Value.TokenExpirationHours));
        var resetLink = string.Format(passwordOptions.Value.ResetLink, resetToken);

        var model = new { reset_link = resetLink, user_name = firstName };

        var emailBody = await emailTemplateService.RenderEmailTemplateAsync(EmailTemplates.ResetPassword, model);

        return new EmailModel
        {
            Email = email,
            Subject = EmailSubjects.ResetPassword,
            Message = emailBody
        };
    }

    public async Task<EmailModel> BuildPasswordChangeConfirmationEmailAsync(string email, string firstName)
    {
        var model = new { user_name = firstName };

        var emailBody = await emailTemplateService.RenderEmailTemplateAsync(EmailTemplates.PasswordChange, model);

        return new EmailModel
        {
            Email = email,
            Subject = EmailSubjects.PasswordChanged,
            Message = emailBody
        };
    }

    public async Task<EmailModel> BuildNewsletterSubscribedEmailAsync(string email, string firstName, string unsubscribeUrl)
    {
        var model = new { user_name = firstName, unsubscribe_url = unsubscribeUrl };

        var emailBody = await emailTemplateService.RenderEmailTemplateAsync(EmailTemplates.SubscriptionConfirmation, model);

        return new EmailModel
        {
            Email = email,
            Subject = EmailSubjects.SubscriptionConfirmation,
            Message = emailBody
        };
    }

    public async Task<EmailModel> BuildShopDeactivationWarningEmailAsync(string email, string shopName, DateTime deactivationDate, string reason)
    {
        var model = new
        {
            shop_name = shopName,
            deactivation_date = deactivationDate.ToString("yyyy-MM-dd"),
            reason = string.IsNullOrEmpty(reason) ? "No reason provided" : reason
        };

        var emailBody = await emailTemplateService.RenderEmailTemplateAsync(EmailTemplates.ShopDeactivationWarning, model);

        return new EmailModel
        {
            Email = email,
            Subject = EmailSubjects.ShopDeactivationWarning,
            Message = emailBody
        };
    }

    public async Task<EmailModel> BuildPlatformAdminActivationEmailAsync(
        string email,
        string firstName,
        string activationLink)
    {
        var templateModel = new
        {
            first_name = firstName,
            activation_link = activationLink,
            current_year = DateTime.UtcNow.Year,
            token_expiration_hours = passwordOptions.Value.TokenExpirationHours
        };

        var body = await emailTemplateService.RenderEmailTemplateAsync(
            EmailTemplates.PlatformAdminActivation,
            templateModel);

        return new EmailModel
        {
            Email = email,
            Subject = EmailSubjects.PlatformAdminActivation,
            Message = body
        };
    }

    public async Task<BulkEmailModel> BuildFeedbackRejectedEmailsAsync(
        List<string> emails,
        string reason)
    {
        var templateModel = new
        {
            reason = reason
        };
        var body = await emailTemplateService.RenderEmailTemplateAsync(
            EmailTemplates.FeedbackRejectionNotification,
            templateModel);

        return new BulkEmailModel
        {
            EmailAddresses = emails,
            Subject = EmailSubjects.FeedbackRejectionNotification,
            Message = body
        };
    }
}
