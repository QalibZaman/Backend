using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UploadInventoryItemsListCommand(IFormFile CsvItemsFile, int UserShopId) : IRequest;

public class UploadInventoryItemsListCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    ICsvFileService csvFileService,
    ICategoryRepository categoryRepository) : IRequestHandler<UploadInventoryItemsListCommand>
{
    public async Task Handle(UploadInventoryItemsListCommand request, CancellationToken cancellationToken)
    {
        if (request.CsvItemsFile.ContentType != "text/csv")
        {
            throw new BadRequestException("The uploaded file must be a CSV file type.");
        }

        if (request.CsvItemsFile == null)
        {
            throw new BadRequestException("CSV file cannot be null.");
        }

        var inventoryItemsData = csvFileService.ConvertEntitiesCsvFileToEntityData<InventoryItemCsvModel>(request.CsvItemsFile);

        var inventoryItems = new List<InventoryItemCsvModel>();

        await foreach (var itemsData in inventoryItemsData)
        {
            inventoryItems.Add(itemsData);
        }

        var databaseInventoryItems = await inventoryItemRepository.GetAllAsync(cancellationToken);
        var inventoryItemsUpc = databaseInventoryItems.Select(x => x.ProductUpc);

        if (inventoryItems.Any(x => inventoryItemsUpc.Contains(x.ProductUpc)))
        {
            throw new ConflictException("Item with such UPC is registered.");
        }

        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        var categoryDictionary = categories.ToDictionary(x => x.Name, x => x.Id);

        foreach (var item in inventoryItems)
        {
            if (!categoryDictionary.TryGetValue(item.ProductCategory, out var categoryId))
            {
                throw new NotFoundException("There is no category with the such name.");
            }

            item.CategoryId = categoryId;
            item.ShopId = request.UserShopId;
        }

        var inventoryItemEntities = inventoryItems.Adapt<IEnumerable<InventoryItemEntity>>();

        await inventoryItemRepository.AddRangeAsync(inventoryItemEntities, cancellationToken);
    }
}