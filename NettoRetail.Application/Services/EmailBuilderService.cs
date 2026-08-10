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
    IOptions<ResetPasswordOptions> passwordOptions) : IEmailBuilderService
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
}