using MediatR;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.ILinkService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Newsletters.Commands;

public record SubscribeToNewsletterCommand(Guid AccountId) : IRequest<Unit>;

public class SubscribeToNewsletterCommandHandler(IAccountRepository accountRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService,
    ILinkService linkService
    ) : IRequestHandler<SubscribeToNewsletterCommand,Unit>
{
    public async Task<Unit> Handle(SubscribeToNewsletterCommand request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetAsync(new GetAccountByIdSpecification(request.AccountId), cancellationToken);
        ValidateCanSubscribe(account);
        account.IsSubscribedToNewsletter = true;
        await accountRepository.UpdateAsync(account, cancellationToken);

        var unsubscribeUrl = linkService.BuildNewsletterUnsubscribeUrl(account.Id);
        var emailModel = await emailBuilderService.BuildNewsletterSubscribedEmailAsync(account.Email, account.FullName, unsubscribeUrl);
        try
        {
            await emailSender.SendAsync(emailModel);
        }
        catch (Exception ex)
        {
            account.IsSubscribedToNewsletter = false;
            await accountRepository.UpdateAsync(account, cancellationToken);
            throw new BadRequestException(
                $"Email send failed: {ex.GetType().FullName} | {ex.Message} | {ex.InnerException?.GetType().FullName} | {ex.InnerException?.Message}");

        }

        return Unit.Value;
    }

    private static void ValidateCanSubscribe(AccountEntity account)
    {
        if (account is null)
            throw new NotFoundException("There is no account with such Id");
        if (account.IsSubscribedToNewsletter)
            throw new BadRequestException("You are already subscribed");

        if (string.IsNullOrWhiteSpace(account.Email))
            throw new BadRequestException("User email is unavailable.");
    }
}