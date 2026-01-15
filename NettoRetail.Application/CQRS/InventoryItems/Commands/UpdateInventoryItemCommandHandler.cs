using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
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
        var spec = new GetInventoryItemByIdAndShopIdSpecification(request.Id, request.ShopId);

        var inventoryItem = await inventoryItemRepository.GetAsync(spec, cancellationToken)
                            ?? throw new NotFoundException("There is no inventory item with such id.");

        await inventoryService.ValidateUpdateAsync(
            request.Model, request.ShopId, request.Id, inventoryItem, cancellationToken);

        var data = new InventoryItemUpdateData(
            ProductUpc: request.Model.ProductUpc!.Value,
            ProductName: request.Model.ProductName!,
            CategoryId: request.Model.CategoryId!.Value,
            BrandId: request.Model.BrandId!.Value,
            Description: request.Model.Description!,
            Price: request.Model.Price!.Value,
            IsActive: request.Model.IsActive!.Value,
            IsPromoted: request.Model.IsPromoted!.Value,
            PromotionPrice: request.Model.PromotionPrice
        );

        inventoryItem.ApplyUpdate(data);

        await inventoryItemRepository.UpdateAsync(inventoryItem, cancellationToken);
    }
}