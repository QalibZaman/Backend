using NettoRetail.Infrastructure.Models;

namespace NettoRetail.Infrastructure.Interfaces;

public interface IEmailSender
{
    Task SendAsync(EmailModel model);
    Task SendBulkAsync(BulkEmailModel model);
}
