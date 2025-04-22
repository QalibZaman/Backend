using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Newsletters.Commands;

public record SubscribeToNewsletterCommand(Guid AccountId, string UnsubscribeUrl) : IRequest;

public class SubscribeToNewsletterCommandHandler(IAccountRepository accountRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService) : IRequestHandler<SubscribeToNewsletterCommand>
{
    public async Task Handle(SubscribeToNewsletterCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetAccountByIdSpecification(request.AccountId);
        var account = await accountRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException($"There is no account with such Id");

        account.IsSubscribedTonewsletter = true;
        await accountRepository.UpdateAsync(account, cancellationToken);

        var emailModel = await emailBuilderService.BuildNewsletterSubscribedEmailAsync(account.Email, account.FirstName, request.UnsubscribeUrl);
        await emailSender.SendAsync(emailModel);
    }
}
