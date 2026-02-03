using NettoRetail.Infrastructure.Models;

namespace NettoRetail.Infrastructure.Interfaces;

public interface IEmailSender
{
    Task SendAsync(EmailModel model, Guid userId);
    Task SendBulkAsync(BulkEmailModel model);
}
