using NettoRetail.Infrastructure.Models;

namespace NettoRetail.Application.Interfaces;
public interface IEmailBuilderService
{
    Task<EmailModel> BuildResetPasswordEmailAsync(string email, string firstName);
    Task<EmailModel> BuildPasswordChangeConfirmationEmailAsync(string email, string firstName);
}
