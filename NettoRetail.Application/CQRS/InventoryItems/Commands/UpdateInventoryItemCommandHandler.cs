using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UpdateInventoryItemCommand(Guid Id, int ShopId, UpdateInventoryItemModel Model) : IRequest;

public class UpdateInventoryItemCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    IInventoryService inventoryService)
    : IRequestHandler<UpdateInventoryItemCommand>
{
    public async Task Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var itemByIdSpecification = new GetInventoryItemByIdAndShopIdSpecification(request.Id, request.ShopId);
        var inventoryItem = await inventoryItemRepository.GetAsync(itemByIdSpecification, cancellationToken)
            ?? throw new NotFoundException("There is no inventory item with such id.");

        await inventoryService.ValidateUpdateAsync(request.Model, request.ShopId, request.Id, inventoryItem, cancellationToken);

        request.Model.Adapt(inventoryItem);

        inventoryItem.LastUpdatedDate = DateTime.UtcNow;
        inventoryItem.Status = inventoryItem.IsActive
            ? InventoryItemStatusEnum.Available
            : InventoryItemStatusEnum.Unavailable;

        await inventoryItemRepository.UpdateAsync(inventoryItem, cancellationToken);
    }
}