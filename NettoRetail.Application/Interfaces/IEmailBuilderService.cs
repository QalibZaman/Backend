using NettoRetail.Infrastructure.Models;

namespace NettoRetail.Application.Interfaces;

public interface IEmailBuilderService
{
    Task<EmailModel> BuildActivateLockedAccountEmailAsync(string email, string firstName, string activationLink);
    Task<EmailModel> BuildAccountDeletedEmailAsync(string email, string firstName);
    Task<EmailModel> BuildFailedDeletionAttemptsEmailAsync(string email, string firstName);
}
