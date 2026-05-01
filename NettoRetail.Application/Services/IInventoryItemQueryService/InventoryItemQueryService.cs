using MapsterMapper;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
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
        model.PageNumber = Math.Max(1, model.PageNumber);
        model.PageSize = model.PageSize <= 0 ? 10 : model.PageSize;

        var builder = await BuildBaseSpecificationAsync(shopId, model, ct);

        builder.AddInclude(x => x.Category);
        builder.AddInclude(x => x.Brand!);

        var skip = PaginationHelper.CalculateSkip(model.PageNumber, model.PageSize);
        var spec = builder.Skip(skip).Take(model.PageSize).Build();

        var (items, total) = await inventoryRepo.GetAllPaginatedAsync(spec, ct);

        return new PaginatedResponse<InventoryItemListResponse>
        {
            Items = mapper.Map<List<InventoryItemListResponse>>(items),
            TotalAmount = total
        };
    }

    private async Task<SpecificationBuilder<InventoryItemEntity>> BuildBaseSpecificationAsync(
        int shopId,
        InventoryItemsSearchModel model,
        CancellationToken ct)
    {
        var builder = await BuildComplexSpecificationAsync(shopId, model, ct);

        if (!string.IsNullOrWhiteSpace(model.Upc))
        {
            builder
                .Add<GetInventoryItemsByShopIdSpecification>(shopId)
                .Add<GetInventoryItemsByPartialUpcSpecification>(model.Upc);
        }

        return builder;
    }

    private async Task<SpecificationBuilder<InventoryItemEntity>> BuildComplexSpecificationAsync(
        int shopId,
        InventoryItemsSearchModel model,
        CancellationToken ct)
    {
        IEnumerable<int> categoryIds = Enumerable.Empty<int>();
        var filter = mapper.Map<GetFilteredInventoryItemsModel>(model);

        if (filter.CategoryId != 0)
        {
            var subtree = await categoryRepo.GetCategorySubtreeAsync(filter.CategoryId, ct);

            if (!subtree.Any())
            {
                throw new NotFoundException(HandlerConstants.Category.CategoryNotFoundMessage);
            }

            categoryIds = subtree.Select(x => x.Id);
        }

        var filterSpec = InventoryItemFilterBuilder.Build(filter, shopId, categoryIds);
        var builder = new SpecificationBuilder<InventoryItemEntity>()
            .AddCriteria(filterSpec.Criteria);

        if (filter.SortingOptions is { Count: > 0 })
        {
            foreach (var option in filter.SortingOptions)
            {
                builder.AddOrderBy(
                    InventoryItemSortingHelper.GetSortingExpression(option.Field),
                    option.IsDescending);
            }
        }
        else if (model.SortOption.HasValue)
        {
            switch (model.SortOption.Value)
            {
                case InventoryItemSortingEnum.PriceLowToHigh:
                    builder.AddOrderBy(x => x.Price, false);
                    break;
                case InventoryItemSortingEnum.PriceHighToLow:
                    builder.AddOrderBy(x => x.Price, true);
                    break;
                case InventoryItemSortingEnum.Newest:
                default:
                    builder.AddOrderBy(x => x.LastUpdatedDate, true);
                    break;
            }
        }
        else
        {
            builder.AddOrderBy(x => x.Id, false);
        }

        return builder;
    }
}