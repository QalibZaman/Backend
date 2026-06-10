using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.FavouriteListItems.Queries;

public sealed record GetFavouriteListQuery(Guid AccountId, int ShopId) : IRequest<GetFavouriteResponse>;

public sealed class GetFavouriteListQueryHandler(
    IFavouriteListItemRepository favouriteRepository,
    IMapper mapper
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

        var distinctItems = favourites
            .Select(x => x.InventoryItem!)
            .DistinctBy(ii => ii.Id)
            .ToList();
        var availableItemCount = distinctItems.Count(ii => !ii.IsOutOfStock);
        var totalPriceAvailable = distinctItems
            .Where(ii => !ii.IsOutOfStock)
            .Sum(ii => ii.Price);

        var items = mapper.Map<List<InventoryItemCardResponse>>(distinctItems);

        return new GetFavouriteResponse
        {
            Items=mapper.Map<List<InventoryItemCardResponse>>(distinctItems),
        };
    }
}
