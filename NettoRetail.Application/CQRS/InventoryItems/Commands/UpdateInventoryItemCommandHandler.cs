using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UpdateInventoryItemCommand(Guid Id, UpdateInventoryItemModel Model) : IRequest;

public class UpdateInventoryItemCommandHandler(IInventoryItemRepository inventoryItemRepository, ICategoryRepository categoryRepository)
    : IRequestHandler<UpdateInventoryItemCommand>
{
    public async Task Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var itemByIdSpecification = new GetItemByIdSpecification(request.Id);
        var inventoryItem = await inventoryItemRepository.GetAsync(itemByIdSpecification, cancellationToken) ?? throw new NotFoundException("There is no inventory item with such id.");

        if (inventoryItem.ProductUpc != request.Model.ProductUpc)
        {
            var productByNameSpecification = new GetProductByUpcSpecification(request.Model.ProductUpc ?? 0);
            var product = await inventoryItemRepository.GetAsync(productByNameSpecification, cancellationToken);

            if (product != null)
            {
                throw new ConflictException("This product UPC is already registered.");
            }
        }

        if (request.Model.CategoryId != null)
        {
            var categoryByIdSpecification = new GetCategoryByIdSpecification(request.Model.CategoryId);
            var category = await categoryRepository.GetAsync(categoryByIdSpecification, cancellationToken) ?? throw new NotFoundException("There is no category with such id.");
        }

        request.Model.Adapt(inventoryItem);

        inventoryItem.LastUpdatedDate = DateTime.UtcNow;
        inventoryItem.Status = inventoryItem.IsActive
            ? InventoryItemStatusEnum.Available
            : InventoryItemStatusEnum.Unavailable;

        await inventoryItemRepository.UpdateAsync(inventoryItem, cancellationToken);
    }
}