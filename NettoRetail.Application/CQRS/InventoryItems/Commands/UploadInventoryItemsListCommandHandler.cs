using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Services;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UploadInventoryItemsListCommand(IFormFile CsvItemsFile, int UserShopId) : IRequest;

public class UploadInventoryItemsListCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    ICsvFileService csvFileService,
    ICategoryRepository categoryRepository,
    IBrandRepository brandRepository,
    IOptions<BatchOptions> batchOptions,
    IValidator<InventoryItemCsvModel> validator
) : IRequestHandler<UploadInventoryItemsListCommand>
{
    private readonly BatchOptions _batchDefaults = batchOptions.Value;

    public async Task Handle(UploadInventoryItemsListCommand request, CancellationToken cancellationToken)
    {
        ValidateCsvFile(request.CsvItemsFile);

        var itemsList = await ReadCsvItemsAsync(request.CsvItemsFile, cancellationToken);
        if (itemsList.Count == 0)
            throw new BadRequestException("The CSV file is empty. Please provide at least one row of data.");

        await ValidateRowsAsync(itemsList, validator, cancellationToken);

        var existingUpcHash = await inventoryItemRepository.GetAllUpcHashAsync(cancellationToken);

        var categoryCache = new Dictionary<string, CategoryEntity>(StringComparer.OrdinalIgnoreCase);
        var brandCache = new Dictionary<string, BrandEntity>(StringComparer.OrdinalIgnoreCase);

        var currentBatch = new List<InventoryItemEntity>(_batchDefaults.BatchSize);

        foreach (var item in itemsList)
        {
            if (existingUpcHash.Contains(item.ProductUpc))
                throw new ConflictException($"Item with UPC {item.ProductUpc} is already registered.");

            var category = await GetOrAddCategoryAsync(
                categoryRepository, categoryCache, item.ProductCategory, request.UserShopId, cancellationToken);

            var brand = await GetOrAddBrandAsync(
                brandRepository, brandCache, item.Brand, request.UserShopId, cancellationToken);

            var entity = MapToEntity(item, category, brand, request.UserShopId);
            currentBatch.Add(entity);

            if (currentBatch.Count >= _batchDefaults.BatchSize)
            {
                await inventoryItemRepository.AddRangeAsync(currentBatch, cancellationToken);
                currentBatch.Clear();
            }
        }

        if (currentBatch.Count > 0)
            await inventoryItemRepository.AddRangeAsync(currentBatch, cancellationToken);
    }


    #region Private Helpers

    private static void ValidateCsvFile(IFormFile file)
    {
        if (file == null)
            throw new BadRequestException("CSV file cannot be null.");

        if (file.ContentType != "text/csv")
            throw new BadRequestException("The uploaded file must be a CSV file type.");
    }

    private static async Task<List<InventoryItemCsvModel>> ReadCsvItemsAsync(
        IFormFile csvFile, CancellationToken cancellationToken)
    {
        var csvService = new CsvFileService();
        var inventoryItemsData = csvService.ConvertEntitiesCsvFileToEntityData<InventoryItemCsvModel>(csvFile);

        var list = new List<InventoryItemCsvModel>();
        await foreach (var item in inventoryItemsData.WithCancellation(cancellationToken))
        {
            list.Add(item);
        }

        return list;
    }

    private static async Task ValidateRowsAsync(
        List<InventoryItemCsvModel> items,
        IValidator<InventoryItemCsvModel> validator,
        CancellationToken cancellationToken)
    {
        var validationErrors = new List<string>();

        foreach (var (item, index) in items.Select((x, i) => (x, i + 1)))
        {
            var result = await validator.ValidateAsync(item, cancellationToken);
            if (!result.IsValid)
            {
                var messages = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
                validationErrors.Add($"Row {index}: {messages}");
            }
        }

        if (validationErrors.Any())
            throw new BadRequestException($"Validation failed:\n{string.Join("\n", validationErrors)}");
    }

    private static async Task<CategoryEntity> GetOrAddCategoryAsync(
        ICategoryRepository repository,
        Dictionary<string, CategoryEntity> cache,
        string categoryName,
        int shopId,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(categoryName, out var cached))
            return cached;

        var spec = new GetCategoriesByNameOrUrlSpecification(categoryName, null, shopId);
        var category = await repository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException($"Category '{categoryName}' not found.");

        cache[categoryName] = category;
        return category;
    }

    private static async Task<BrandEntity> GetOrAddBrandAsync(
        IBrandRepository repository,
        Dictionary<string, BrandEntity> cache,
        string brandName,
        int shopId,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(brandName, out var cached))
            return cached;

        var specBuilder = new SpecificationBuilder<BrandEntity>();
        specBuilder
            .Add<GetBrandsByNameSpecification>(brandName)
            .Add<GetBrandsByShopIdSpecification>(shopId);

        var spec = specBuilder.Build();
        var brand = await repository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException($"Brand '{brandName}' not found.");

        cache[brandName] = brand;
        return brand;
    }

    private static InventoryItemEntity MapToEntity(
        InventoryItemCsvModel item,
        CategoryEntity category,
        BrandEntity brand,
        int shopId)
    {
        var entity = item.Adapt<InventoryItemEntity>();
        entity.Category = category;
        entity.CategoryId = category.Id;
        entity.Brand = brand;
        entity.BrandId = brand.Id;
        entity.ShopId = shopId;
        return entity;
    }

    #endregion
}
