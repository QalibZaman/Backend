using MapsterMapper;
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
    IInventoryItemRepository inventoryRepository,
    IMapper mapper
) : IRequestHandler<ResolveGuestFavouritesQuery, GetFavouriteResponse>
{
    public async Task<GetFavouriteResponse> Handle(ResolveGuestFavouritesQuery request, CancellationToken cancellationToken)
    {
        if (request.ProductIds == null || request.ProductIds.Count == 0)
        {
            return new GetFavouriteResponse
            {
                Items = Array.Empty<InventoryItemCardResponse>(),
            };
        }

        var spec = new SpecificationBuilder<InventoryItemEntity>()
            .Add<GetInventoryItemsByIdsAndShopSpecification>(request.ProductIds, request.ShopId)
            .AddInclude(x => x.Category)
            .AddInclude(x => x.Images)
            .Build();

        var itemsDb = await inventoryRepository.GetManyBySpecAsync(spec, cancellationToken);

        var distinctItems = itemsDb
            .DistinctBy(ii => ii.Id)
            .ToList();

       
        return new GetFavouriteResponse
        {
            Items = mapper.Map<List<InventoryItemCardResponse>>(distinctItems),
        };
    }
}
