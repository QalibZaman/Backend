using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UploadInventoryItemsListCommand(IFormFile CsvItemsFile, int UserShopId) : IRequest;

public class UploadInventoryItemsListCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    ICsvFileService csvFileService,
    ICategoryRepository categoryRepository,
    IOptions<BatchOptions> batchOptions) : IRequestHandler<UploadInventoryItemsListCommand>
{
    private BatchOptions _batchDefaults = batchOptions.Value;

    public async Task Handle(UploadInventoryItemsListCommand request, CancellationToken cancellationToken)
    {
        if (request.CsvItemsFile == null)
        {
            throw new BadRequestException("CSV file cannot be null.");
        }

        if (request.CsvItemsFile.ContentType != "text/csv")
        {
            throw new BadRequestException("The uploaded file must be a CSV file type.");
        }

        var inventoryItemsData = csvFileService.ConvertEntitiesCsvFileToEntityData<InventoryItemCsvModel>(request.CsvItemsFile);

        var inventoryItemsUpcHash = await inventoryItemRepository.GetAllUpcHashAsync(cancellationToken);
        var categoryDictionary = await categoryRepository.GetCategoryNameAndIdDictionaryAsync(cancellationToken);

        var currentBatch = new List<InventoryItemEntity>(_batchDefaults.BatchSize);

        await foreach (var item in inventoryItemsData)
        {
            if (inventoryItemsUpcHash.Contains(item.ProductUpc))
            {
                throw new ConflictException("Item with such UPC is registered.");
            }

            if (!categoryDictionary.TryGetValue(item.ProductCategory, out var categoryId))
            {
                throw new NotFoundException("There is no category with the such name.");
            }

            item.CategoryId = categoryId;
            item.ShopId = request.UserShopId;

            var entity = item.Adapt<InventoryItemEntity>();
            currentBatch.Add(entity);

            if (currentBatch.Count >= _batchDefaults.BatchSize)
            {
                await inventoryItemRepository.AddBulkAsync(currentBatch, cancellationToken);
                currentBatch.Clear();
            }
        }

        if (currentBatch.Count > 0)
        {
            await inventoryItemRepository.AddBulkAsync(currentBatch, cancellationToken);
        }
    }
}