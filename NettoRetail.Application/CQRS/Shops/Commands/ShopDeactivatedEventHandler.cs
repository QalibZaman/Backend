using MediatR;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record ShopDeactivatedEvent(int ShopId) : INotification;

// INotifier notifier
public class ShopDeactivatedEventHandler(
    ISessionRepository sessionRepo,
    ISessionTerminationService sessionTerminator) : INotificationHandler<ShopDeactivatedEvent>
{
    public async Task Handle(ShopDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        var sessions = await sessionRepo.GetManyBySpecAsync(
            new GetSessionsByShopIdSpecification(notification.ShopId),
            cancellationToken);

        foreach (var session in sessions)
        {
            await sessionTerminator.TerminateSessionsAsync(session.Id, cancellationToken);

            //await notifier.NotifyAsync(session.AccountId, "Your session has ended due to shop deactivation.", cancellationToken);
        }
    }
}