using NettoRetail.Infrastructure.Models;

namespace NettoRetail.Application.Interfaces;

public interface IEmailBuilderService
{
    Task<EmailModel> BuildNewsletterSubscribedEmailAsync(string email, string fullName,Guid accountId);
}
