using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.FavouriteListItems.Queries;

public sealed record ResolveGuestFavouritesQuery(
    IReadOnlyCollection<Guid> ProductIds,
    int ShopId
) : IRequest<GetFavouriteResponse>;

public sealed class ResolveGuestFavouritesQueryHandler(
    IInventoryItemRepository inventoryRepository
) : IRequestHandler<ResolveGuestFavouritesQuery, GetFavouriteResponse>
{
    public async Task<GetFavouriteResponse> Handle(ResolveGuestFavouritesQuery request, CancellationToken cancellationToken)
    {
        if (request.ProductIds == null || request.ProductIds.Count == 0)
        {
            return new GetFavouriteResponse
            {
                Items = Array.Empty<InventoryItemCardResponse>(),
                AvailableItemCount = 0,
                TotalPriceAvailable = 0m
            };
        }

        var specificationBuilder = new SpecificationBuilder<InventoryItemEntity>();
        specificationBuilder
            .Add<GetInventoryItemsByIdsAndShop>(request.ProductIds, request.ShopId)
            .AddInclude(x => x.Category)
            .AddInclude(x => x.Images);

        var itemsDb = await inventoryRepository.GetManyBySpecAsync(
            specificationBuilder.Build(),
            cancellationToken);

        var items = itemsDb
            .DistinctBy(ii => ii.Id)
            .Select(ii =>
            {
                var isOutOfStock = ii.AvailableAmount <= 0;
                var thumb = ii.Images?.OrderBy(img => img.Order).FirstOrDefault()?.BlobUrl ?? string.Empty;

                return new InventoryItemCardResponse
                {
                    Id = ii.Id,
                    ProductName = ii.ProductName,
                    CategoryName = ii.Category?.Name ?? string.Empty,
                    ThumbnailUrl = thumb,
                    Price = ii.Price,
                    PromotionPrice = ii.PromotionPrice,
                    AvailableAmount = ii.AvailableAmount,
                    IsOutOfStock = isOutOfStock
                };
            })
            .ToList();

        var total = items
            .Where(i => !i.IsOutOfStock)
            .Select(i => i.PromotionPrice ?? i.Price)
            .DefaultIfEmpty(0m)
            .Sum();

        return new GetFavouriteResponse
        {
            Items = items,
            AvailableItemCount = items.Count(i => !i.IsOutOfStock),
            TotalPriceAvailable = total
        };
    }
}
