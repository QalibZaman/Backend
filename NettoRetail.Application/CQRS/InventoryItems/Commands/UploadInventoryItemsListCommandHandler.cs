using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Services.IBrandService;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UploadInventoryItemsListCommand(IFormFile CsvItemsFile, int UserShopId) : IRequest;

public class UploadInventoryItemsListCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    ICsvFileService csvFileService,
    IValidator<List<InventoryItemCsvModel>> listValidator,
    ICategoryLevelManagementService categoryService,
    IBrandService brandService,
    IMapper mapper
) : IRequestHandler<UploadInventoryItemsListCommand>
{

    public async Task Handle(UploadInventoryItemsListCommand request, CancellationToken cancellationToken)
    {
        var itemsList = await csvFileService.ReadCsvItemsAsync(request.CsvItemsFile, cancellationToken);

        await listValidator.ValidateAsync(itemsList, cancellationToken);

        var requestedUpcs = itemsList.Select(item => item.ProductUpc).Distinct().ToList();
        var existingItems = await inventoryItemRepository.GetAllBySpecAsync(
            new GetInventoryItemsByUpcsSpecification(requestedUpcs),
            cancellationToken);
        var existingByUpc = existingItems.ToDictionary(item => item.ProductUpc);

        var categoryCache = new Dictionary<string, CategoryEntity>(StringComparer.OrdinalIgnoreCase);
        var brandCache = new Dictionary<string, BrandEntity>(StringComparer.OrdinalIgnoreCase);

        var entitiesToAdd = new List<InventoryItemEntity>(itemsList.Count);
        var entitiesToUpdate = new List<InventoryItemEntity>();

        foreach (var item in itemsList)
        {
            if (existingByUpc.TryGetValue(item.ProductUpc, out var existingItem) &&
                existingItem.ShopId != request.UserShopId)
                throw new ConflictException($"Item with UPC {item.ProductUpc} is already registered in another shop.");

            var category = await categoryService.GetCategoryByNameWithCacheAsync(
                 categoryCache, item.ProductCategory, request.UserShopId, cancellationToken);

            var brand = await brandService.GetBrandByNameWithCacheAsync(
                 brandCache, item.Brand, request.UserShopId, cancellationToken);

            if (existingItem != null)
            {
                ApplyCsvUpdate(existingItem, item, category, brand, request.UserShopId);
                entitiesToUpdate.Add(existingItem);
                continue;
            }

            var entity = mapper.Map<InventoryItemEntity>(item);
            entity.Category = category;
            entity.Brand = brand;
            entity.BrandId = brand.Id;
            entity.CategoryId = category.Id;
            entity.ShopId = request.UserShopId;
            entity.LastUpdatedDate = DateTime.UtcNow;

            entitiesToAdd.Add(entity);
        }

        if (entitiesToAdd.Count > 0)
            await inventoryItemRepository.AddRangeAsync(entitiesToAdd, cancellationToken);

        if (entitiesToUpdate.Count > 0)
            await inventoryItemRepository.UpdateManyAsync(entitiesToUpdate, cancellationToken);
    }

    private static void ApplyCsvUpdate(
        InventoryItemEntity entity,
        InventoryItemCsvModel item,
        CategoryEntity category,
        BrandEntity brand,
        int shopId)
    {
        entity.ProductUpc = item.ProductUpc;
        entity.ProductName = item.ProductName;
        entity.ProductLocation = item.ProductLocation;
        entity.AvailableAmount = item.AvailableAmount;
        entity.ReservedAmount = item.ReservedAmount;
        entity.OnHandAmount = item.OnHandAmount;
        entity.Price = item.Price;
        entity.Status = item.Status;
        entity.Description = item.Description;
        entity.Category = category;
        entity.CategoryId = category.Id;
        entity.Brand = brand;
        entity.BrandId = brand.Id;
        entity.ShopId = shopId;
        entity.LastUpdatedDate = DateTime.UtcNow;
    }
}
