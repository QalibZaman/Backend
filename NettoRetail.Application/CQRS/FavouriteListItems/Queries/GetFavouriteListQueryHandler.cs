using Mapster;
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
        var spec = new SpecificationBuilder<FavouriteListItemEntity>()
            .Add<GetFavouriteListByAccountAndShop>(request.AccountId, request.ShopId)
            .AddInclude(x => x.InventoryItem!)
            .AddInclude(x => x.InventoryItem!.Category)
            .AddInclude(x => x.InventoryItem!.Images)
            .Build();

        var favourites = await favouriteRepository.GetManyBySpecAsync(spec, cancellationToken);

        var items = favourites
            .Select(x => x.InventoryItem!)
            .DistinctBy(ii => ii.Id)
            .Adapt<List<InventoryItemCardResponse>>();

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
