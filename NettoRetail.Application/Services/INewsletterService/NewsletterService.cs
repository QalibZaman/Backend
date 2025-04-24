using NettoRetail.Application.Models.Email;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.INewsletterService;

public class NewsletterService : INewsletterService
{
    private readonly IEmailSender _emailSender;
    private readonly IAccountRepository _accountRepository;

    public NewsletterService(IEmailSender emailSender, IAccountRepository accountRepository)
    {
        _emailSender = emailSender;
        _accountRepository = accountRepository;
    }

    public async Task SendNewsletterAsync(EmailModelRequest model)
    {
        var specification = new GetAccountBySubscribedToNewsletter(true);
        var emails = await _accountRepository.GetAccountEmailsAsync(specification, CancellationToken.None);

        var bulkEmailModel = new BulkEmailModel { EmailAddresses = emails, Message = model.Message, Subject = model.Subject };

        await _emailSender.SendBulkAsync(bulkEmailModel);
    }
}
