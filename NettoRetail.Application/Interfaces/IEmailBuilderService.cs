using NettoRetail.Infrastructure.Models;

namespace NettoRetail.Application.Interfaces;

public interface IEmailBuilderService
{
    Task<EmailModel> BuildResetPasswordEmailAsync(string email, string firstName);
    Task<EmailModel> BuildPasswordChangeConfirmationEmailAsync(string email, string firstName);
    Task<EmailModel> BuildNewsletterSubscribedEmailAsync(string email, string fullName, string unsubscribeUrl);
    Task<EmailModel> BuildShopDeactivationWarningEmailAsync(string email, string shopName, DateTime deactivationDate, string reason);
    Task<EmailModel> BuildPlatformAdminActivationEmailAsync(string email, string firstName, string activationLink);
    Task<BulkEmailModel> BuildFeedbackRejectedEmailsAsync(List<string> emails, string reason);
    Task<EmailModel> BuildPlatformAdminActivatedEmailAsync(string email, string firstName);
    Task<EmailModel> BuildCustomerAccountActivationEmailAsync(string email, string firstName, string link);
}
