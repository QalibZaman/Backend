using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.IFavouriteListItemsService;

public record FavouriteListBulkAddResult(
    int AddedCount,
    CartEntity? LastCart,
    CartItemEntity? LastLine);

public interface IFavouriteListItems
{
    Task<List<Guid>> ResolveTargetIdsAsync(AddToCartFromFavouritesCommand request, object? favouriteItemsSpecification,CancellationToken cancellationToken = default);

    Task<CartEntity> AddManyToCartAsync(Guid accountId, IEnumerable<Guid> inventoryItemIds, CancellationToken cancellationToken = default);
}