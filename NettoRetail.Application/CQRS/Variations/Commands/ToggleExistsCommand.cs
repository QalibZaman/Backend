using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Variations.Commands;

public record ToggleExistsCommand(Guid Id, int ShopId) : IRequest<Unit>;

public class ToggleExistsCommandHandler(
    IInventoryItemRepository inventoryItemRepository)
    : IRequestHandler<ToggleExistsCommand, Unit>
{
    public async Task<Unit> Handle(ToggleExistsCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetInventoryItemByIdAndShopIdSpecification(request.Id, request.ShopId);

        var variation = await inventoryItemRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Variation not found");

        EnsureVariation(variation);

        variation.Exists = !variation.Exists;
        if (!variation.Exists)
            variation.IsActive = false;

        variation.Status = variation.IsActive && variation.Exists
            ? InventoryItemStatusEnum.Available
            : InventoryItemStatusEnum.Unavailable;
        variation.LastUpdatedDate = DateTime.UtcNow;

        await inventoryItemRepository.UpdateAsync(variation, cancellationToken);

        return Unit.Value;
    }

    private static void EnsureVariation(InventoryItemEntity item)
    {
        if (!item.IsVariation)
            throw new BadRequestException("This item is not a variation.");
    }
}
