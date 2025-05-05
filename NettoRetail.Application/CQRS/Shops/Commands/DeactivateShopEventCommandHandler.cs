using Azure.Core;
using MediatR;
using NettoRetail.Application.CQRS.Shops.Commands;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record DeactivateShopEventCommand(int ShopId, string Reason, DateTime? DeactivationDate) : INotification;

public class DeactivateShopEventCommandHandler(
    IShopRepository shopRepository,
    ISessionRepository sessionRepo,
    ISessionTerminationService sessionTerminator,
    IEmailSender emailSender) : INotificationHandler<DeactivateShopEventCommand>
{
    public async Task Handle(DeactivateShopEventCommand request, CancellationToken cancellationToken)
    {
        DateTime deactivationDate = request.DeactivationDate ?? DateTime.UtcNow.AddDays(14);

        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken);

        shop.DeactivationReason = request.Reason;
        shop.DeactivationDate = deactivationDate;
        // Send email
        var email = new EmailModel
        {
            Email = shop.Email,
            Subject = "Shop Deactivation Scheduled",
            Message = $"""
                Dear {shop.Name},<br/><br/>
                Your shop is scheduled to be deactivated on <b>{request.DeactivationDate:yyyy-MM-dd}</b>.<br/>
                Reason: <i>{request.Reason}</i>.<br/><br/>
                Please take the necessary actions before the deactivation date.<br/><br/>
                Best regards,<br/>
                Support Team
            """
        };

        await emailSender.SendAsync(email);

        // Session termination
        var sessions = await sessionRepo.GetManyBySpecAsync(
            new GetSessionsByShopIdSpecification(request.ShopId),
            cancellationToken);

        foreach (var session in sessions)
        {
            await sessionTerminator.TerminateSessionsAsync(session.Id, cancellationToken);
        }
    }
}
