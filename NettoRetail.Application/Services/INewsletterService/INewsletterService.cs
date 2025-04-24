using NettoRetail.Application.Models.Email;

namespace NettoRetail.Application.Services.INewsletterService;

public interface INewsletterService
{
    Task SendNewsletterAsync(EmailModelRequest model);
}
