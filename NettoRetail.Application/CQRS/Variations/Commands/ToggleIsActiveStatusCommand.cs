using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Variations.Commands;

public record ToggleIsActiveStatusCommand(Guid Id, int ShopId) : IRequest<Unit>;

public class ToggleIsActiveStatusCommandHandler(
    IInventoryItemRepository inventoryItemRepository)
    : IRequestHandler<ToggleIsActiveStatusCommand, Unit>
{
    public async Task<Unit> Handle(ToggleIsActiveStatusCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetInventoryItemByIdAndShopIdSpecification(request.Id, request.ShopId);

        var item = await inventoryItemRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Variation not found");

        EnsureVariation(item);

        if (!item.Exists)
            throw new BadRequestException("Item supposed to be existed");

        item.IsActive = !item.IsActive;
        item.Status = item.IsActive
            ? InventoryItemStatusEnum.Available
            : InventoryItemStatusEnum.Unavailable;
        item.LastUpdatedDate = DateTime.UtcNow;

        await inventoryItemRepository.UpdateAsync(item, cancellationToken);

        return Unit.Value;
    }

    private static void EnsureVariation(InventoryItemEntity item)
    {
        if (!item.IsVariation)
            throw new BadRequestException("This item is not a variation.");
    }
}
