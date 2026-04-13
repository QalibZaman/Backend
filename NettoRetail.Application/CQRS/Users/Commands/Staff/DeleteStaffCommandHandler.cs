using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.INotificationService;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record DeleteStaffCommand(Guid Id, int ShopId) : IRequest;
public class DeleteStaffCommandHandler(IStaffRepository staffRepository,
    ISessionTerminationService sessionTerminationService,
    IUnitOfWork unitOfWork,
    INotificationService notificationService)
    : IRequestHandler<DeleteStaffCommand>
{
    public async Task Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByIdSpecification(request.Id);
        var account = await staffRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException(StaffConstants.StaffMemberNotFound);

        if (account.ShopId != request.ShopId)
            throw new ForbiddenException(StaffConstants.StaffMemberAccessDenied);

        await DeleteStaffAsync(account, cancellationToken);
    }

    private async Task DeleteStaffAsync(
        StaffEntity account,
        CancellationToken cancellationToken)
    {
        var isOnline = account.IsOnline();

        using var transaction = unitOfWork.BeginTransaction();

        await TerminateSessionsIfOnlineAsync(isOnline, account.Id, cancellationToken);

        await staffRepository.DeleteAsync(account, cancellationToken);

        transaction.Commit();

        await NotifyIfOnlineAsync(isOnline, account.Id, cancellationToken);
    }

    private Task TerminateSessionsIfOnlineAsync(
        bool isOnline,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        if (!isOnline)
            return Task.CompletedTask;

        return sessionTerminationService.TerminateSessionsAsync(accountId, cancellationToken);
    }

    private Task NotifyIfOnlineAsync(
        bool isOnline,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        if (!isOnline)
            return Task.CompletedTask;

        return notificationService.NotifyUserAsync(
            accountId.ToString(),
            StaffConstants.AccountDeletedNotification);
    }
}
