using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UpdateInventoryItemCommand(Guid Id, int ShopId, UpdateInventoryItemModel Model) : IRequest<Unit>;

public class UpdateInventoryItemCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    IInventoryService inventoryService)
    : IRequestHandler<UpdateInventoryItemCommand,Unit>
{
    public async Task<Unit> Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetInventoryItemByIdAndShopIdSpecification(request.Id, request.ShopId);

        var inventoryItem = await inventoryItemRepository.GetAsync(spec, cancellationToken)
                            ?? throw new NotFoundException("There is no inventory item with such id.");

        await inventoryService.ValidateUpdateAsync(
            request.Model, request.ShopId, request.Id, inventoryItem, cancellationToken);

        var data = request.Model.Adapt<InventoryItemUpdateData>();
        inventoryItem.ApplyUpdate(data);
        await inventoryItemRepository.UpdateAsync(inventoryItem, cancellationToken);
        return Unit.Value;
    }
}