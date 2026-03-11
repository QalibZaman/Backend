using System.Text.RegularExpressions;
using MapsterMapper;
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
    ICategoryRepository categoryRepo,
    IMapper mapper) : IInventoryItemQueryService
{
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

    private PaginatedResponse<InventoryItemListResponse> CreateResponse(ICollection<InventoryItemEntity> items, int total)
    {
        return new PaginatedResponse<InventoryItemListResponse>
        {
            Items = mapper.Map<List<InventoryItemListResponse>>(items),
            TotalAmount = total
        };
    }

    private async Task<PaginatedResponse<InventoryItemListResponse>> HandleQuickSearchAsync(
        int shopId,
        InventoryItemsSearchModel model,
        CancellationToken ct)
    {
        var builder = new SpecificationBuilder<InventoryItemEntity>()
            .Add<GetInventoryItemsByShopIdSpecification>(shopId);

        if (Regex.IsMatch(model.UpcOrName, @"^\d{1,12}$"))
            builder.Add<GetInventoryItemsByPartialUpcSpecification>(model.UpcOrName);
        else
            builder.Add<GetInventoryItemsByPartialNameSpecification>(model.UpcOrName);

        var skip = PaginationHelper.CalculateSkip(model.PageNumber, model.PageSize);
        var spec = builder.Skip(skip).Take(model.PageSize).Build();

        var (items, total) = await inventoryRepo.GetAllPaginatedAsync(spec, ct);

        return CreateResponse(items, total);
    }

    private async Task<PaginatedResponse<InventoryItemListResponse>> HandleSimpleSortAsync(
        int shopId,
        InventoryItemsSearchModel model,
        CancellationToken ct)
    {
        var builder = new SpecificationBuilder<InventoryItemEntity>()
           .Add<GetInventoryItemPriceSortingSpecification>(shopId, model.SortOption!.Value);
        var skip = PaginationHelper.CalculateSkip(model.PageNumber, model.PageSize);
        var spec = builder.Skip(skip).Take(model.PageSize).Build();

        var (sortedItems, total) = await inventoryRepo.GetAllPaginatedAsync(spec, ct);

        return CreateResponse(sortedItems, total);
    }

    private async Task<PaginatedResponse<InventoryItemListResponse>> HandleFilteredAndSortedAsync(
        int shopId,
        InventoryItemsSearchModel model,
        CancellationToken ct)
    {
        var filter = mapper.Map<GetFilteredInventoryItemsModel>(model);
        var skip = PaginationHelper.CalculateSkip(model.PageNumber, model.PageSize);

        var categoryIds = Enumerable.Empty<int>();
        if (filter.CategoryId != 0)
        {
            var categories = await categoryRepo.GetCategorySubtreeAsync(filter.CategoryId, ct);
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
        else
        {
            specBuilder.AddOrderBy(i => i.Id, false);
        }

        var finalSpec = specBuilder.Skip(skip).Take(model.PageSize).Build();

        var (items, total) = await inventoryRepo.GetAllPaginatedAsync(finalSpec, ct);

        return CreateResponse(items, total);
    }
}
