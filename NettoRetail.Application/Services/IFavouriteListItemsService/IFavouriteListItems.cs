using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.FavouriteListItems;

public interface IFavouriteListItemsService
{
    Task<List<Guid>> ResolveTargetIdsAsync(
        AddToCartFromFavouritesCommand request,
        Specification<FavouriteListItemEntity> favouriteItemsSpecification,
        CancellationToken cancellationToken = default);

    Task<CartEntity> AddManyToCartAsync(
        Guid accountId,
        IEnumerable<Guid> inventoryItemIds,
        CancellationToken cancellationToken = default);
}
