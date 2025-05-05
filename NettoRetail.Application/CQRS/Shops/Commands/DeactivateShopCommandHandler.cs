using System.ComponentModel.DataAnnotations;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Infrastructure.Services;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record DeactivateShopCommand(int ShopId, string Reason, DateTime? DeactivationDate) : IRequest;

public class DeactivateShopCommandHandler(
    IShopRepository shopRepository,
    IMediator mediator,
    IEmailSender emailSender,
    ISessionRepository sessionRepo,
    ISessionTerminationService sessionTerminator) : IRequestHandler<DeactivateShopCommand>
{
    public async Task Handle(DeactivateShopCommand request, CancellationToken cancellationToken)
    {
        DateTime deactivationDate = request.DeactivationDate ?? DateTime.UtcNow.AddDays(14);

        // Get the shop
        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken)
            ?? throw new NotFoundException("Shop not found");

        // Update
        shop.IsActive = false;
        shop.DeactivationReason = request.Reason;
        shop.DeactivationDate = deactivationDate;

        // session
        var sessions = await sessionRepo.GetManyBySpecAsync(
        new GetSessionsByShopIdSpecification(request.ShopId),
        cancellationToken);

        foreach (var session in sessions)
        {
            await sessionTerminator.TerminateSessionsAsync(session.Id, cancellationToken);
        }

        // send email
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

        // Save
        await shopRepository.UpdateAsync(shop, cancellationToken);

        // Publish the event for deactivation
        await mediator.Publish(new DeactivateShopCommand(shop.Id, request.Reason, deactivationDate), cancellationToken);
    }
}
