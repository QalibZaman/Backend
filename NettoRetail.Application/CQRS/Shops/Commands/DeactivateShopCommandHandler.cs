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
    IMediator mediator) : IRequestHandler<DeactivateShopCommand>
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

        // Save
        await shopRepository.UpdateAsync(shop, cancellationToken);

        // Publish the event for deactivation and for Email sending
        await mediator.Publish(new DeactivateShopEventCommand(shop.Id, request.Reason, deactivationDate), cancellationToken);
    }
}
