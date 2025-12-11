using Mapster;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IInventoryItemQueryService;
public class InventoryItemQueryService(
    IInventoryItemRepository inventoryRepo,
    ICategoryRepository categoryRepo) : IInventoryItemQueryService
{
    private readonly IInventoryItemRepository _inventoryRepo = inventoryRepo;
    private readonly ICategoryRepository _categoryRepo = categoryRepo;

    public async Task<PaginatedResponse<InventoryItemListResponse>> GetInventoryItemsAsync(
        int shopId,
        InventoryItemsSearchModel model,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(model.UpcOrName))
            return await HandleQuickSearchAsync(shopId, model, ct);

        if (model.SortOption.HasValue && model.SortingOptions is null)
            return await HandleSimpleSortAsync(shopId, model, ct);

        return await HandleFilteredAndSortedAsync(shopId, model, ct);
    }

    private async Task<PaginatedResponse<InventoryItemListResponse>> HandleQuickSearchAsync(
        int shopId,
        InventoryItemsSearchModel model,
        CancellationToken ct)
    {
        var builder = new SpecificationBuilder<InventoryItemEntity>()
            .Add<GetInventoryItemsByShopIdSpecification>(shopId);

        if (long.TryParse(model.UpcOrName, out var upc))
            builder.Add<GetInventoryItemsByPartialUpcSpecification>(upc);
        else
            builder.Add<GetInventoryItemsByPartialNameSpecification>(model.UpcOrName);

        var skip = (model.PageNumber - 1) * model.PageSize;
        var spec = builder.Skip(skip).Take(model.PageSize).Build();

        var (items, total) = await _inventoryRepo.GetAllPaginatedAsync(spec, ct);

        return new PaginatedResponse<InventoryItemListResponse>
        {
            Items = items.Adapt<List<InventoryItemListResponse>>(),
            TotalAmount = total
        };
    }

    private async Task<PaginatedResponse<InventoryItemListResponse>> HandleSimpleSortAsync(
        int shopId,
        InventoryItemsSearchModel model,
        CancellationToken ct)
    {
        var builder = new SpecificationBuilder<InventoryItemEntity>()
           .Add<GetInventoryItemPriceSortingSpecification>(shopId, model.SortOption!.Value);
        var skip = model.PageNumber > 0 ? (model.PageNumber - 1) * model.PageSize : 0;
        var spec = builder.Skip(skip).Take(model.PageSize).Build();

        var (sortedItems, total) = await _inventoryRepo.GetAllPaginatedAsync(spec, ct);

        return new PaginatedResponse<InventoryItemListResponse>
        {
            Items = sortedItems.Adapt<List<InventoryItemListResponse>>(),
            TotalAmount = total
        };
    }

    private async Task<PaginatedResponse<InventoryItemListResponse>> HandleFilteredAndSortedAsync(
        int shopId,
        InventoryItemsSearchModel model,
        CancellationToken ct)
    {
        var filter = model.Adapt<GetFilteredInventoryItemsModel>();
        var skip = (model.PageNumber - 1) * model.PageSize;

        var categoryIds = Enumerable.Empty<int>();
        if (filter.CategoryId != 0)
        {
            var categories = await _categoryRepo.GetCategorySubtreeAsync(filter.CategoryId, ct);
            categoryIds = categories.Select(c => c.Id);
        }

        var filterSpec = InventoryItemFilterBuilder.Build(filter, shopId, categoryIds);

        var specBuilder = new SpecificationBuilder<InventoryItemEntity>();

        specBuilder.AddCriteria(filterSpec.Criteria);

        if (filter.SortingOptions is { Count: > 0 })
        {
            foreach (var option in filter.SortingOptions)
            {
                specBuilder.AddOrderBy(
                    InventoryItemSortingHelper.GetSortingExpression(option.Field),
                    option.IsDescending);
            }
        }

        var finalSpec = specBuilder.Skip(skip).Take(model.PageSize).Build();

        var (items, total) = await _inventoryRepo.GetAllPaginatedAsync(finalSpec, ct);

        return new PaginatedResponse<InventoryItemListResponse>
        {
            Items = items.Adapt<List<InventoryItemListResponse>>(),
            TotalAmount = total
        };
    }
}