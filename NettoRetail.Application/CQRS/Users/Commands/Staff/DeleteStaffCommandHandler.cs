using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.INotificationService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
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
            ?? throw new NotFoundException(StaffConstants.StaffMemberNotFound);

        if (account.ShopId != request.ShopId)
            throw new ForbiddenException(StaffConstants.StaffMemberAccessDenied);

        await DeleteStaffAsync(account, cancellationToken);
    }

    private async Task DeleteStaffAsync(
        AccountEntity account,
        CancellationToken cancellationToken)
    {
        var isOnline = account.Status == StaffStatusEnum.Online;

        using var transaction = unitOfWork.BeginTransaction();
        
        if (isOnline)
            {
                await sessionTerminationService.TerminateSessionsAsync(account.Id, cancellationToken);
            }

        await accountRepository.DeleteAsync(account, cancellationToken);
        transaction.Commit();

        if (isOnline)
        {
            try
            {
                await notificationService.NotifyUserAsync(
                    account.Id.ToString(),
                    StaffConstants.AccountDeletedNotification);
            }
            catch
            {
                throw new BadRequestException("There is problem happend during the Notifing user");
            }
        }
    }
}
