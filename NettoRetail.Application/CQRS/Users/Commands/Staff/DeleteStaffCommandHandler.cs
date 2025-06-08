using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.INotificationService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record DeleteStaffCommand(Guid Id, int ShopId) : IRequest;
public class DeleteStaffCommandHandler(IAccountRepository accountRepository,
    ISessionTerminationService sessionTerminationService,
    IUnitOfWork unitOfWork,
    INotificationService notificationService)
    : IRequestHandler<DeleteStaffCommand>
{
    public async Task Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetAccountByIdSpecification(request.Id);
        var account = await accountRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("Account not found");

        if (account.ShopId != request.ShopId)
            throw new BadRequestException("You are not allowed to access this staff member.");

        // begin transaction to ensure atomicity between session termination and account deletion.
        // this prevents partial updates in case of failure for example if sessions are terminated but account deletion fails.
        var transaction = unitOfWork.BeginTransaction();
        try
        {
            await sessionTerminationService.TerminateSessionsAsync(account.Id, cancellationToken);
            await notificationService.NotifyUserAsync(account.Id.ToString(), "Your session has been terminated for security reasons");
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
