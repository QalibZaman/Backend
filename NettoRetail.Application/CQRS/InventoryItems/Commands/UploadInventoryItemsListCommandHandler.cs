using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Services.IBranService;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UploadInventoryItemsListCommand(IFormFile CsvItemsFile, int UserShopId) : IRequest;

public class UploadInventoryItemsListCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    ICsvFileService csvFileService,
    ICategoryRepository categoryRepository,
    IBrandRepository brandRepository,
    IOptions<BatchOptions> batchOptions,
    IValidator<InventoryItemCsvModel> validator,
    ICategoryLevelManagementService categoryService,
    IBrandService brandService
) : IRequestHandler<UploadInventoryItemsListCommand>
{
    private readonly BatchOptions _batchDefaults = batchOptions.Value;

    public async Task Handle(UploadInventoryItemsListCommand request, CancellationToken cancellationToken)
    {
        var itemsList = await csvFileService.ReadCsvItemsAsync(request.CsvItemsFile, cancellationToken);
        await csvFileService.ValidateRowsFromCsvFileAsync(itemsList, validator, cancellationToken);

        var existingUpcHash = await inventoryItemRepository.GetAllUpcHashAsync(cancellationToken);

        var categoryCache = new Dictionary<string, CategoryEntity>(StringComparer.OrdinalIgnoreCase);
        var brandCache = new Dictionary<string, BrandEntity>(StringComparer.OrdinalIgnoreCase);

        var entitiesToAdd = new List<InventoryItemEntity>(itemsList.Count);

        foreach (var item in itemsList)
        {
            if (existingUpcHash.Contains(item.ProductUpc))
                throw new ConflictException($"Item with UPC {item.ProductUpc} is already registered.");

            var category = await categoryService.GetCategoryByNameWithCacheAsync(
                 categoryCache, item.ProductCategory, request.UserShopId, cancellationToken);

            var brand = await brandService.GetBrandByNameWithCacheAsync(
                 brandCache, item.Brand, request.UserShopId, cancellationToken);

            var entity = item.Adapt<InventoryItemEntity>();
            entity.Category = category;
            entity.Brand = brand;
            entity.BrandId = brand?.Id;
            entity.ShopId = request.UserShopId;

            entitiesToAdd.Add(entity);
        }

        if (entitiesToAdd.Count > 0)
            await inventoryItemRepository.AddRangeAsync(entitiesToAdd, cancellationToken);
    }
}
