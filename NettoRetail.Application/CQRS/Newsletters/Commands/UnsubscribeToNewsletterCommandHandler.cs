using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Newsletters.Commands;

public record UnsubscribeToNewsletterCommand(Guid AccountId) : IRequest;

public class UnsubscribeToNewsletterCommandHandler(IStaffRepository accountRepository) : IRequestHandler<UnsubscribeToNewsletterCommand>
{
    public async Task Handle(UnsubscribeToNewsletterCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByIdSpecification(request.AccountId);
        var account = await accountRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException($"There is no account with such Id");

        if (account.IsSubscribedToNewsletter == false)
        {
            throw new BadRequestException("You are already unsubscribed");
        }

        account.IsSubscribedToNewsletter = false;
        await accountRepository.UpdateAsync(account, cancellationToken);
    }
}
