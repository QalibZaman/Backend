using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.FavouriteListItems.Queries;

public sealed record GetFavouriteListQuery(Guid AccountId, int ShopId) : IRequest<GetFavouriteResponse>;

public sealed class GetFavouriteListQueryHandler(
    IFavouriteListItemRepository favouriteRepository
) : IRequestHandler<GetFavouriteListQuery, GetFavouriteResponse>
{
    public async Task<GetFavouriteResponse> Handle(GetFavouriteListQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<FavouriteListItemEntity>();
        specificationBuilder
            .Add<GetFavouriteListByAccountAndShop>(request.AccountId, request.ShopId)
            .AddInclude(x => x.InventoryItem!)
            .AddInclude(x => x.InventoryItem!.Category)
            .AddInclude(x => x.InventoryItem!.Images);

        var favourites = await favouriteRepository.GetManyBySpecAsync(
            specificationBuilder.Build(),
            cancellationToken);

        var items = favourites
            .Select(x => x.InventoryItem!)
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
