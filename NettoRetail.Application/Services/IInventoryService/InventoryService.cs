using MapsterMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;
using NotFoundException = NettoRetail.Application.Exceptions.NotFoundException;

namespace NettoRetail.Application.Services.IInventoryService;

public class InventoryService(
    IOptions<InventoryItemOptions> options,
    IInventoryItemRepository inventoryItemRepository,
    ICsvFileService csvFileService,
    ICategoryRepository categoryRepository,
    IBrandRepository brandRepository,
    ILogger<InventoryService> logger,
    IMapper mapper) : IInventoryService
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
            .Skip(PaginationHelper.CalculateSkip(pageModel.PageNumber, pageModel.PageSize))
            .Take(pageModel.PageSize)
            .ToList();

        var productResponses = mapper.Map<List<InventoryItemListResponse>>(pagedProduct);

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

        var newUpc = model.ProductUpc!;
        if (inventoryItem.ProductUpc != newUpc)
        {
            await ValidateUpcUnique(newUpc, shopId, inventoryItem.Id, cancellationToken);
        }

        ValidateFinalState(model,inventoryItem);
    }

    public static void ValidateFinalState(UpdateInventoryItemModel model,
        InventoryItemEntity inventoryItem)
    {
        var availabe = model.AvailableAmount ?? inventoryItem.AvailableAmount;
        var reserved = model.ReservedAmount ?? inventoryItem.ReservedAmount;
        var onHand = model.OnHandAmount ?? inventoryItem.OnHandAmount;

        if (availabe < reserved)
            throw new BadRequestException("Reserved amount cannot exceed the available amount");

        if (reserved + availabe > onHand)
            throw new BadRequestException("The sum of Reserved Amount and Available Amount should not exceed the On-hand Amount.");
    }

    public List<InventoryItemEntity> BuildVariationInventoryItemEntities(ICollection<VariationModel> models, InventoryItemEntity parentItem)
    {
        if (models == null)
            return new List<InventoryItemEntity>();

        List<InventoryItemEntity> result = [];

        foreach (var item in models)
        {
            var variant = mapper.Map<InventoryItemEntity>(item);

            variant.AvailableAmount = 0;
            variant.IsVariation = true;
            variant.Price = parentItem.Price;
            variant.ShopId = parentItem.ShopId;
            variant.ParentItemId = parentItem.Id;

            variant.Description = parentItem.Description;
            variant.CategoryId = parentItem.CategoryId;
            variant.BrandId = parentItem.BrandId;

            result.Add(variant);
        }

        return result;
    }
    
    private async Task ValidateCategoryExists(int categoryId, int shopId, CancellationToken ct)
    {
        var spec = new GetCategoryByIdAndShopIdSpecification(categoryId, shopId);
        if (!await categoryRepository.Exists(spec, ct))
            throw new NotFoundException("There is no category with such id.");
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

    public async Task<ToggleLinkProductsResponse> ToggleLinkProductsAsync(Guid parentId, Guid childId, int shopId, Dictionary<string, string>? conflictResolutions, CancellationToken cancellationToken)
    {
        bool isLinkedNow = false;
        if (parentId == childId)
            throw new ConflictException("Product cannot be parent and child of itself");

        var parent = await inventoryItemRepository.GetAsync(
            new GetInventoryItemByIdSpecification(parentId),
            cancellationToken) ?? throw new NotFoundException("Parent product not found");

        var child = await inventoryItemRepository.GetAsync(
            new GetInventoryItemWithChildrenSpecification(childId),
            cancellationToken) ?? throw new NotFoundException("Child product not found");

        if (parent.ShopId != shopId || child.ShopId != shopId)
            throw new BadRequestException("Both products must belong to your shop");

        if (parent.ParentItemId.HasValue)
            throw new ConflictException("A child product cannot be used as a parent");

        if (child.ParentItemId.HasValue && child.ParentItemId.Value != parentId)
            throw new ConflictException("Child product already belongs to another parent");

        if (child.ChildItems!.Any())
            throw new ConflictException("A parent product cannot be linked as a child");

        if (child.ParentItemId == parentId)
        {
            // Unlink
            child.ParentItemId = null;
        }
        else if (child.ParentItemId is null)
        {
            // Link — detect conflicts (AC#4, AC#5)
            var conflicts = DetectAttributeConflicts(parent, child);

            if (conflicts.Count > 0 && conflictResolutions is null)
            {
                // Return conflicts for frontend to display conflict resolution UI (ID-242)
                return new ToggleLinkProductsResponse(childId, child.ProductName, child.ProductUpc, false, true, conflicts);
            }

            // Apply attribute inheritance with conflict resolution
            ApplyAttributeInheritance(parent, child, conflicts, conflictResolutions);

            child.ParentItemId = parentId;
            isLinkedNow = true;
        }

        await inventoryItemRepository.UpdateAsync(child, cancellationToken);
        return new ToggleLinkProductsResponse(childId, child.ProductName, child.ProductUpc, isLinkedNow);
    }

    private static List<AttributeConflict> DetectAttributeConflicts(InventoryItemEntity parent, InventoryItemEntity child)
    {
        var conflicts = new List<AttributeConflict>();

        if (parent.CategoryId != child.CategoryId)
            conflicts.Add(new AttributeConflict("CategoryId", parent.CategoryId.ToString(), child.CategoryId.ToString()));

        if (parent.BrandId != child.BrandId)
            conflicts.Add(new AttributeConflict("BrandId", parent.BrandId?.ToString(), child.BrandId?.ToString()));

        if (parent.Price != child.Price)
            conflicts.Add(new AttributeConflict("Price", parent.Price.ToString(), child.Price.ToString()));

        if (parent.Description != child.Description)
            conflicts.Add(new AttributeConflict("Description", parent.Description, child.Description));

        return conflicts;
    }

    private static void ApplyAttributeInheritance(InventoryItemEntity parent, InventoryItemEntity child, List<AttributeConflict> conflicts, Dictionary<string, string>? resolutions)
    {
        if (conflicts.Count == 0)
        {
            // No conflicts — inherit all from parent (AC#4)
            child.CategoryId = parent.CategoryId;
            child.BrandId = parent.BrandId;
            child.Price = parent.Price;
            child.Description = parent.Description;
            return;
        }

        // Apply user's chosen resolutions (AC#5)
        foreach (var conflict in conflicts)
        {
            var useParentValue = resolutions != null
                && resolutions.TryGetValue(conflict.AttributeName, out var choice)
                && choice.Equals("parent", StringComparison.OrdinalIgnoreCase);

            switch (conflict.AttributeName)
            {
                case "CategoryId" when useParentValue:
                    child.CategoryId = parent.CategoryId;
                    break;
                case "BrandId" when useParentValue:
                    child.BrandId = parent.BrandId;
                    break;
                case "Price" when useParentValue:
                    child.Price = parent.Price;
                    break;
                case "Description" when useParentValue:
                    child.Description = parent.Description;
                    break;
            }
        }
    }

    private async Task ValidateBrandExists(int brandId, int shopId, CancellationToken ct)
    {
        var spec = new GetBrandsByIdAndShopIdSpecification(brandId, shopId);
        if (!await brandRepository.Exists(spec, ct))
            throw new NotFoundException("There is no brand with such id.");
    }

    private async Task ValidateUpc(UpdateInventoryItemModel model, int shopId, Guid id, InventoryItemEntity inventoryItem, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(model.ProductUpc) && inventoryItem.ProductUpc != model.ProductUpc)
        {
            var spec = new GetProductByUpcSpecification(model.ProductUpc);
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
    
    private async Task ValidateUpcUnique(string upc, int shopId, Guid currentItemId, CancellationToken ct)
    {
        var spec = new GetProductByUpcSpecification(upc);
        var existing = await inventoryItemRepository.GetAsync(spec, ct);

        if (existing is not null && existing.Id != currentItemId)
            throw new ConflictException("This product UPC is already registered.");
    }
}
