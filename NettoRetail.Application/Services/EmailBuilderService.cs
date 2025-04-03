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
    IOptions<ResetPasswordOptions> passwordOptions
) : IEmailBuilderService
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
}
