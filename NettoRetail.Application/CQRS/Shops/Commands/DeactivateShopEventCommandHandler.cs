using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.INotificationService;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record DeactivateShopEventCommand(int ShopId, string Reason, DateTime? DeactivationDate, Guid AccountId) : INotification;

public class DeactivateShopEventCommandHandler(
    IShopRepository shopRepository,
    ISessionRepository sessionRepo,
    ISessionTerminationService sessionTerminator,
    IEmailBuilderService emailBuilder,
    IEmailSender emailSender,
    INotificationService notification) : INotificationHandler<DeactivateShopEventCommand>
{
    public async Task Handle(DeactivateShopEventCommand request, CancellationToken cancellationToken)
    {
        DateTime deactivationDate = request.DeactivationDate ?? DateTime.UtcNow.AddDays(14);

        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken) ?? throw new NotFoundException("Shop not found during event dispatch");

        shop.DeactivationReason = request.Reason;
        shop.DeactivationDate = deactivationDate;

        var email = await emailBuilder.BuildShopDeactivationWarningEmailAsync(
            shop.Email, shop.Name, deactivationDate, request.Reason);

        await emailSender.SendAsync(email, request.AccountId);

        var sessions = await sessionRepo.GetManyBySpecAsync(
            new GetSessionsByShopIdSpecification(request.ShopId),
            cancellationToken);

        foreach (var session in sessions)
        {
            await sessionTerminator.TerminateSessionsAsync(session.Id, cancellationToken);
        }

        await notification.NotifyShopGroupAsync(shop.Id.ToString(), $"Shop {shop.Id} has been deactivated.");
        await notification.NotifyAllAsync($"Shop {shop.Id} is now inactive due to: {shop.DeactivationReason}");
    }
}
