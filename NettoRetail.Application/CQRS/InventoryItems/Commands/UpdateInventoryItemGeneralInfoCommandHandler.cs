using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UpdateInventoryItemGeneralInfoCommand(Guid Id, UpdateInventoryItemGeneralInfoModel Model) : IRequest;

public class UpdateInventoryItemGeneralInfoCommandHandler(IInventoryItemRepository inventoryItemRepository, ICategoryRepository categoryRepository)
    : IRequestHandler<UpdateInventoryItemGeneralInfoCommand>
{
    public async Task Handle(UpdateInventoryItemGeneralInfoCommand request, CancellationToken cancellationToken)
    {
        var itemByIdSpecification = new GetItemByIdSpecification(request.Id);
        var inventoryItem = await inventoryItemRepository.GetAsync(itemByIdSpecification, cancellationToken) ?? throw new NotFoundException("There is no inventory item with such id.");

        if (inventoryItem.ProductUpc != request.Model.ProductUpc)
        {
            var productByNameSpecification = new GetProductByUpcSpecification(request.Model.ProductUpc);
            var product = await inventoryItemRepository.GetAsync(productByNameSpecification, cancellationToken);

            if (product != null)
            {
                throw new ConflictException("This product UPC is already registered.");
            }
        }

        var categoryByIdSpecification = new GetCategoryByIdSpecification(request.Model.CategoryId);
        var category = await categoryRepository.GetAsync(categoryByIdSpecification, cancellationToken) ?? throw new NotFoundException("There is no category with such id.");

        request.Model.Adapt(inventoryItem);
        inventoryItem.LastUpdatedDate = DateTime.UtcNow;
        inventoryItem.Status = inventoryItem.IsActive
            ? InventoryItemStatusEnum.Available
            : InventoryItemStatusEnum.Unavailable;

        await inventoryItemRepository.UpdateAsync(inventoryItem, cancellationToken);
    }
}