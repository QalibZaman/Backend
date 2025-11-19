using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IInventoryService;

public class InventoryService(
    IOptions<InventoryItemOptions> options,
    IInventoryItemRepository inventoryItemRepository,
    ICsvFileService csvFileService,
    ILogger<InventoryService> logger) : IInventoryService
{
    private readonly InventoryItemOptions _options = options.Value;
    private readonly IInventoryItemRepository _inventoryItemRepository = inventoryItemRepository;
    private readonly ICsvFileService _csvFileService = csvFileService;
    private readonly ILogger<InventoryService> _logger = logger;

    public bool IsItemNew(InventoryItemEntity item) =>
        item.LastUpdatedDate >= DateTime.UtcNow.AddDays(-_options.NewItemDaysThreshold);

    public bool IsItemOutOfStock(InventoryItemEntity item) =>
        item.AvailableAmount <= 0;

    public bool IsItemOnSale(InventoryItemEntity item) =>
        item.PromotionPrice.HasValue;

    public async Task<PaginatedResponse<InventoryItemListResponse>> GetShopItemsBySearchAsync(
        string query, int shopId, PaginationRequest pageModel, CancellationToken cancellationToken)
    {
        pageModel ??= new PaginationRequest();

        var inventoryItemSpecBuilder = new SpecificationBuilder<InventoryItemEntity>();
        inventoryItemSpecBuilder
            .Add<GetInventoryItemsByPartialNameSpecification>(query)
            .Add<GetInventoryItemsByShopIdSpecification>(shopId)
            .AddInclude(x => x.Brand)
            .AddInclude(x => x.Category);

        var specification = inventoryItemSpecBuilder.Build();
        var products = await _inventoryItemRepository.GetAllBySpecAsync(specification, cancellationToken);

        var total = products.Count();

        var pagedProduct = products
            .Skip((pageModel.PageNumber - 1) * pageModel.PageSize)
            .Take(pageModel.PageSize)
            .ToList();

        var productResponses = pagedProduct.Adapt<List<InventoryItemListResponse>>();

        return new PaginatedResponse<InventoryItemListResponse>
        {
            Items = productResponses,
            TotalAmount = total,
        };
    }

    public async Task<byte[]> DownloadInventoryItemsCsvAsync(int userShopId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting inventory items CSV download for ShopId: {ShopId}", userShopId);

        var itemByRelatedCategorySpecification = new GetInventoryItemsRelatedCategories(userShopId);
        itemByRelatedCategorySpecification.AddInclude(i => i.Brand!);

        var inventoryItems = await _inventoryItemRepository.GetAllBySpecAsync(itemByRelatedCategorySpecification, cancellationToken);

        if (!inventoryItems.Any())
        {
            _logger.LogWarning("No inventory items found for ShopId: {ShopId}", userShopId);
            throw new NotFoundException("There are no items in the list.");
        }

        var inventoryItemsData = await _csvFileService.GetEntitiesCsvData<InventoryItemCsvModel, InventoryItemEntity>(
            inventoryItems, cancellationToken);

        _logger.LogInformation("Successfully generated CSV for {Count} items (ShopId: {ShopId})", inventoryItems.Count(), userShopId);

        return inventoryItemsData;
    }
}
