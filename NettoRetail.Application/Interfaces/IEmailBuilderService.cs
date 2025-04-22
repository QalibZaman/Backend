using NettoRetail.Infrastructure.Models;

namespace NettoRetail.Application.Interfaces;

public interface IEmailBuilderService
{
    Task<EmailModel> BuildResetPasswordEmailAsync(string email, string firstName);
    Task<EmailModel> BuildPasswordChangeConfirmationEmailAsync(string email, string firstName);
    Task<EmailModel> BuildNewsletterSubscribedEmailAsync(string email, string firstName, string unsubscribeUrl);
}
