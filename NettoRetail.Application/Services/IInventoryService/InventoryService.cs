using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.DataAccess.Specifications.Categories;
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
    ICategoryRepository categoryRepository,
    IBrandRepository brandRepository,
    ILogger<InventoryService> logger) : IInventoryService
{
    public bool IsItemNew(InventoryItemEntity item) =>
        item.LastUpdatedDate >= DateTime.UtcNow.AddDays(-options.Value.NewItemDaysThreshold);

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
        var products = await inventoryItemRepository.GetAllBySpecAsync(specification, cancellationToken);

        IEnumerable<InventoryItemEntity> inventoryItemEntities = products.ToList();
        var total = inventoryItemEntities.Count();

        var pagedProduct = inventoryItemEntities
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
        logger.LogInformation("Starting inventory items CSV download for ShopId: {ShopId}", userShopId);

        var itemByRelatedCategorySpecification = new GetInventoryItemsRelatedCategories(userShopId);
        itemByRelatedCategorySpecification.AddInclude(i => i.Brand!);

        var inventoryItems = await inventoryItemRepository.GetAllBySpecAsync(itemByRelatedCategorySpecification, cancellationToken);

        if (!inventoryItems.Any())
        {
            logger.LogWarning("No inventory items found for ShopId: {ShopId}", userShopId);
            throw new NotFoundException("There are no items in the list.");
        }

        var inventoryItemsData = await csvFileService.GetEntitiesCsvData<InventoryItemCsvModel, InventoryItemEntity>(
            inventoryItems, cancellationToken);

        logger.LogInformation("Successfully generated CSV for {Count} items (ShopId: {ShopId})", inventoryItems.Count(), userShopId);

        return inventoryItemsData;
    }
    public async Task ValidateUpdateAsync(UpdateInventoryItemModel model, int shopId, Guid id, InventoryItemEntity inventoryItem, CancellationToken cancellationToken)
    {
        await ValidateUpc(model, shopId, id, inventoryItem, cancellationToken);
        await ValidateCategory(model, shopId, id, inventoryItem, cancellationToken);
        await ValidateBrand(model, shopId, id, inventoryItem, cancellationToken);
        ValidatePromotionPrice(model, shopId, id, inventoryItem, cancellationToken);

    }

    public List<InventoryItemEntity> BuildVariationInventoryItemEntities(ICollection<VariationModel> models, InventoryItemEntity parentItem)
    {
        if (models == null)
            return new List<InventoryItemEntity>();

        List<InventoryItemEntity> result = [];

        foreach (var item in models)
        {
            var variant = item.Adapt<InventoryItemEntity>();

            variant.AvailableAmount = 0;
            variant.IsVariation = true;
            variant.Price = parentItem.Price;
            variant.ShopId = parentItem.ShopId;
            variant.ParentItemId = parentItem.Id;
            //In US currently nothing said about description, brand and category, but currently it not nullable. In future that must be clarified.
            variant.Description = parentItem.Description;
            variant.CategoryId = parentItem.CategoryId;
            variant.BrandId = parentItem.BrandId;

            result.Add(variant);
        }

        return result;
    }

    public async Task<bool> AreAllItemsBelongToShopAsync(int shopId, IEnumerable<Guid> inventoryItemsIds, CancellationToken cancellationToken)
    {
        if (inventoryItemsIds == null || !inventoryItemsIds.Any())
            throw new BadRequestException("There must be at least one item id");

        var spec = new GetItemsByIdsSpecification(inventoryItemsIds);
        var items = inventoryItemRepository.GetRangeByIdsAsync(spec, cancellationToken);

        int foundCount = 0;

        await foreach (var item in items.WithCancellation(cancellationToken))
        {
            if (item.ShopId != shopId)
                return false;

            foundCount++;
        }

        var areAllItemnsExists = foundCount == inventoryItemsIds.Count();
        if (!areAllItemnsExists) throw new BadRequestException("All items must be unique");

        return true;
    }

    private async Task ValidateUpc(UpdateInventoryItemModel model, int shopId, Guid id, InventoryItemEntity inventoryItem, CancellationToken cancellationToken)
    {
        if (model.ProductUpc.HasValue && inventoryItem.ProductUpc != model.ProductUpc)
        {
            var spec = new GetProductByUpcSpecification(model.ProductUpc.Value);
            if (await inventoryItemRepository.Exists(spec, cancellationToken))
                throw new ConflictException("This product UPC is already registered.");
        }
    }

    private async Task ValidateCategory(UpdateInventoryItemModel model, int shopId, Guid id, InventoryItemEntity inventoryItem, CancellationToken cancellationToken)
    {
        if (model.CategoryId is null) return;

        var spec = new GetCategoryByIdAndShopIdSpecification(model.CategoryId.Value, shopId);
        if (!await categoryRepository.Exists(spec, cancellationToken))
            throw new NotFoundException("There is no category with such id.");
    }

    private async Task ValidateBrand(UpdateInventoryItemModel model, int shopId, Guid id, InventoryItemEntity inventoryItem, CancellationToken cancellationToken)
    {
        if (model.BrandId is null) return;

        var spec = new GetBrandsByIdAndShopIdSpecification(model.BrandId.Value, shopId);
        if (!await brandRepository.Exists(spec, cancellationToken))
            throw new NotFoundException("There is no brand with such id.");
    }

    private void ValidatePromotionPrice(UpdateInventoryItemModel model, int shopId, Guid id, InventoryItemEntity inventoryItem, CancellationToken cancellationToken)
    {
        if (model.IsPromoted == true && model.PromotionPrice == null)
            throw new BadRequestException("Promotion price must be provided for promoted products.");

        if (model.IsPromoted == false && model.PromotionPrice != null)
            throw new BadRequestException("Promotion price cannot be provided for non-promoted products.");

        if (model.PromotionPrice != null && model.PromotionPrice >= model.Price)
            throw new BadRequestException("Promotion price must be less than the regular price.");
    }
}
