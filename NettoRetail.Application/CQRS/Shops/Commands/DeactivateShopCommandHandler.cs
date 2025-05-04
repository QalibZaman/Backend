using System.ComponentModel.DataAnnotations;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands;
public record DeactivateShopCommand(int ShopId, string Reason, DateTime DeactivationDate) : IRequest;

public class DeactivateShopCommandHandler(
    IShopRepository shopRepository,
    IMediator mediator) : IRequestHandler<DeactivateShopCommand>
{
    public async Task Handle(DeactivateShopCommand request, CancellationToken cancellationToken)
    {
        // Validate reason and deactivation date
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new ValidationException("Deactivation reason is required.");

        if (request.DeactivationDate <= DateTime.UtcNow.Date)
            throw new ValidationException("Deactivation date must be in the future.");

        // Get the shop
        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken)
            ?? throw new NotFoundException("Shop not found");

        // Update
        shop.IsActive = false;
        shop.DeactivationReason = request.Reason;
        shop.DeactivationDate = request.DeactivationDate;

        // Save
        await shopRepository.UpdateAsync(shop, cancellationToken);

        // Publish
        await mediator.Publish(new ShopDeactivatedEvent(shop.Id), cancellationToken);
    }
}
