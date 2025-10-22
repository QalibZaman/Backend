using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.DataAccess.Specifications.Categories;
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
    IValidator<InventoryItemCsvModel> validator
) : IRequestHandler<UploadInventoryItemsListCommand>
{
    private readonly BatchOptions _batchDefaults = batchOptions.Value;

    public async Task Handle(UploadInventoryItemsListCommand request, CancellationToken cancellationToken)
    {
        if (request.CsvItemsFile == null)
            throw new BadRequestException("CSV file cannot be null.");

        if (request.CsvItemsFile.ContentType != "text/csv")
            throw new BadRequestException("The uploaded file must be a CSV file type.");

        var inventoryItemsData = csvFileService.ConvertEntitiesCsvFileToEntityData<InventoryItemCsvModel>(request.CsvItemsFile);

        var itemsList = new List<InventoryItemCsvModel>();
        await foreach (var item in inventoryItemsData.WithCancellation(cancellationToken))
        {
            itemsList.Add(item);
        }

        if (itemsList.Count == 0)
            throw new BadRequestException("The CSV file is empty. Please provide at least one row of data.");

        var validationErrors = new List<string>();
        foreach (var (item, index) in itemsList.Select((x, i) => (x, i + 1)))
        {
            FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(item, cancellationToken);

            if (!result.IsValid)
            {
                var messages = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
                validationErrors.Add($"Row {index}: {messages}");
            }
        }

        if (validationErrors.Any())
            throw new BadRequestException($"Validation failed:\n{string.Join("\n", validationErrors)}");

        var inventoryItemsUpcHash = await inventoryItemRepository.GetAllUpcHashAsync(cancellationToken);

        var currentBatch = new List<InventoryItemEntity>(_batchDefaults.BatchSize);

        try
        {
            foreach (var item in itemsList)
            {
                if (inventoryItemsUpcHash.Contains(item.ProductUpc))
                    throw new ConflictException($"Item with UPC {item.ProductUpc} is already registered.");

                var categorySpec = new GetCategoriesByNameOrUrlSpecification(item.ProductCategory, null, request.UserShopId);
                var category = await categoryRepository.GetAsync(categorySpec, cancellationToken);
                if (category is null)
                    throw new NotFoundException($"Category '{item.ProductCategory}' not found.");

                var brandSpec = new GetBrandsByNameSpecification(item.Brand);
                var brand = await brandRepository.GetAsync(brandSpec, cancellationToken);
                if (brand is null)
                    throw new NotFoundException($"Brand '{item.Brand}' not found.");

                var entity = item.Adapt<InventoryItemEntity>();
                entity.Category = category;
                entity.CategoryId=category.Id;
                entity.Brand = brand;
                entity.BrandId = brand.Id;
                entity.ShopId = request.UserShopId;
                currentBatch.Add(entity);

                if (currentBatch.Count >= _batchDefaults.BatchSize)
                {
                    await inventoryItemRepository.AddRangeAsync(currentBatch, cancellationToken);
                    currentBatch.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            throw new BadRequestException($"CSV parsing failed. Please ensure values like enum 'Status' are valid. {ex.Message}");
        }

        if (currentBatch.Count > 0)
            await inventoryItemRepository.AddRangeAsync(currentBatch, cancellationToken);
    }
}
