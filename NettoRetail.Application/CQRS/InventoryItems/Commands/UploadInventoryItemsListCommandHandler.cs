using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Services.IBrandService;
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
    IValidator<List<InventoryItemCsvModel>> listValidator,
    ICategoryLevelManagementService categoryService,
    IBrandService brandService
) : IRequestHandler<UploadInventoryItemsListCommand>
{

    public async Task Handle(UploadInventoryItemsListCommand request, CancellationToken cancellationToken)
    {
        var itemsList = await csvFileService.ReadCsvItemsAsync(request.CsvItemsFile, cancellationToken);

        await listValidator.ValidateAndThrowAsync(itemsList, cancellationToken);

        var existingUpcHash = await inventoryItemRepository.GetAllUpcHashAsync(cancellationToken);

        var csvUpcSet = new HashSet<long>();
        var categoryCache = new Dictionary<string, CategoryEntity>(StringComparer.OrdinalIgnoreCase);
        var brandCache = new Dictionary<string, BrandEntity>(StringComparer.OrdinalIgnoreCase);

        var entitiesToAdd = new List<InventoryItemEntity>(itemsList.Count);

        foreach (var item in itemsList)
        {
            if (!csvUpcSet.Add(item.ProductUpc))
                throw new ConflictException($"Duplicate UPC {item.ProductUpc} in CSV file.");

            if (existingUpcHash.Contains(item.ProductUpc))
                throw new ConflictException($"Item with UPC {item.ProductUpc} is already registered.");

            var category = await categoryService.GetCategoryByNameWithCacheAsync(
                categoryCache, item.ProductCategory, request.UserShopId, cancellationToken);

            var brand = await brandService.GetBrandByNameWithCacheAsync(
                brandCache, item.Brand, request.UserShopId, cancellationToken);

            if (category is null) throw new NotFoundException($"Category '{item.ProductCategory}' not found.");
            if (brand is null) throw new NotFoundException($"Brand '{item.Brand}' not found.");

            var entity = item.Adapt<InventoryItemEntity>();

            entity.ShopId = request.UserShopId;

            entity.Category = category;
            entity.CategoryId = category.Id;

            entity.Brand = brand;
            entity.BrandId = brand.Id;

            entitiesToAdd.Add(entity);
        }

        if (entitiesToAdd.Count > 0)
            await inventoryItemRepository.AddRangeAsync(entitiesToAdd, cancellationToken);

    }
}
