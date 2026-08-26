using NettoRetail.Infrastructure.Models;

namespace NettoRetail.Application.Interfaces;

public interface IEmailBuilderService
{
    Task<EmailModel> BuildNewsletterSubscribedEmailAsync(string email, string fullName, Guid accountId);
    Task<EmailModel> BuildNewsletterUnsubscribedEmailAsync(string email, string fullName, int year);
    Task<EmailModel> BuildActivateLockedAccountEmailAsync(string email, string firstName, string activationLink);
    Task<EmailModel> BuildAccountDeletedEmailAsync(string email, string firstName);
    Task<EmailModel> BuildFailedDeletionAttemptsEmailAsync(string email, string firstName);
    Task<EmailModel> BuildResetPasswordEmailAsync(string email, string firstName);
}
