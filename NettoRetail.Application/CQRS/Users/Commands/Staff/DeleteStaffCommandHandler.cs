using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record DeleteStaffCommand(Guid Id) : IRequest;
public class DeleteStaffCommandHandler(IAccountRepository accountRepository,
    ISessionTerminationService sessionTerminationService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteStaffCommand>
{
    public async Task Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetAccountByIdSpecification(request.Id);
        var account = await accountRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("Account not found");

        var transaction = unitOfWork.BeginTransaction();
        try
        {
            await sessionTerminationService.TerminateSessionsAsync(account.Id, cancellationToken);
            await accountRepository.DeleteAsync(specification, cancellationToken);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
