using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.Services.IInventoryService;

public class InventoryService(
    IOptions<InventoryItemOptions> options,
    IInventoryItemRepository inventoryItemRepository,
    ICsvFileService csvFileService,
    ILogger<InventoryService> logger) : IInventoryService
{
    private readonly InventoryItemOptions _options = options.Value;

    public bool IsItemNew(InventoryItemEntity item) => item.LastUpdatedDate >= DateTime.UtcNow.AddDays(-_options.NewItemDaysThreshold);
    public bool IsItemOutOfStock(InventoryItemEntity item) => item.AvailableAmount <= 0;
    public bool IsItemOnSale(InventoryItemEntity item) => item.PromotionPrice.HasValue;
    public async Task<byte[]> DownloadInventoryItemsCsvAsync(int userShopId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting inventory items CSV download for ShopId: {ShopId}", userShopId);

        var itemByRelatedCategorySpecification = new GetInventoryItemsRelatedCategories(userShopId);
        itemByRelatedCategorySpecification.AddInclude(i => i.Brand!);

        var inventoryItems = await inventoryItemRepository.GetAllBySpecAsync(itemByRelatedCategorySpecification, cancellationToken);

        if (!inventoryItems.Any())
        {
            logger.LogWarning("No inventory items found for ShopId: {ShopId}", userShopId);
            throw new NotFoundException("There are no items in the list.");
        }

        var inventoryItemsData = await csvFileService.GetEntitiesCsvData<InventoryItemCsvModel, InventoryItemEntity>(inventoryItems, cancellationToken);

        logger.LogInformation("Successfully generated CSV for {Count} items (ShopId: {ShopId})", inventoryItems.Count(), userShopId);

        return inventoryItemsData;
    }
}
