using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Http.Features;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.INotificationService;
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
    INotificationService notification) : IRequestHandler<DeactivateShopCommand>
{
    public async Task Handle(DeactivateShopCommand request, CancellationToken cancellationToken)
    {
        DateTime deactivationDate = request.DeactivationDate ?? DateTime.UtcNow.AddDays(14);

        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken)
            ?? throw new NotFoundException("Shop not found");

        shop.IsActive = false;
        shop.DeactivationReason = request.Reason;
        shop.DeactivationDate = deactivationDate;

        await shopRepository.UpdateAsync(shop, cancellationToken);

        await mediator.Publish(new DeactivateShopEventCommand(shop.Id, request.Reason, deactivationDate), cancellationToken);

        await notification.NotifyShopGroupAsync(shop.Id.ToString(), $"Shop {shop.Id} has been deactivated.");
        await notification.NotifyAllAsync($"Shop {shop.Id} is now inactive due to: {shop.DeactivationReason}");

    }
}
