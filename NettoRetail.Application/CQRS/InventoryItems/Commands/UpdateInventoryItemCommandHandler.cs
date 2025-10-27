using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UpdateInventoryItemCommand(Guid Id, int ShopId, UpdateInventoryItemModel Model) : IRequest;

public class UpdateInventoryItemCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    ICategoryRepository categoryRepository,
    IBrandRepository brandRepository)
    : IRequestHandler<UpdateInventoryItemCommand>
{
    public async Task Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var itemByIdSpecification = new GetInventoryItemByIdAndShopIdSpecification(request.Id, request.ShopId);
        var inventoryItem = await inventoryItemRepository.GetAsync(itemByIdSpecification, cancellationToken)
            ?? throw new NotFoundException("There is no inventory item with such id.");

        await ValidateRequestModel(request, inventoryItem, cancellationToken);

        request.Model.Adapt(inventoryItem);

        inventoryItem.LastUpdatedDate = DateTime.UtcNow;
        inventoryItem.Status = inventoryItem.IsActive
            ? InventoryItemStatusEnum.Available
            : InventoryItemStatusEnum.Unavailable;

        await inventoryItemRepository.UpdateAsync(inventoryItem, cancellationToken);
    }

    private async Task ValidateRequestModel(UpdateInventoryItemCommand request, InventoryItemEntity inventoryItem, CancellationToken cancellationToken)
    {
        if (request.Model.ProductUpc.HasValue && inventoryItem.ProductUpc != request.Model.ProductUpc)
        {
            var productByNameSpecification = new GetProductByUpcSpecification(request.Model.ProductUpc.Value);
            var productExists = await inventoryItemRepository.Exists(productByNameSpecification, cancellationToken);

            if (productExists)
                throw new ConflictException("This product UPC is already registered.");
        }

        if (request.Model.CategoryId != null)
        {
            var categoryByIdSpecification = new GetCategoryByIdAndShopIdSpecification(request.Model.CategoryId.Value, request.ShopId);
            var categoryExists = await categoryRepository.Exists(categoryByIdSpecification, cancellationToken);

            if (!categoryExists)
                throw new NotFoundException("There is no category with such id.");
        }

        if (request.Model.BrandId != null)
        {
            var brandByIdSpecification = new GetBrandsByIdAndShopIdSpecification(request.Model.BrandId.Value, request.ShopId);
            var brandExists = await brandRepository.Exists(brandByIdSpecification, cancellationToken);

            if (!brandExists)
                throw new NotFoundException("There is no brand with such id.");
        }
    }
}