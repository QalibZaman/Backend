using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IFavouriteListItemsService;

public class FavouriteListItems(
    IFavouriteListItemRepository favouriteListRepo,
    CartService cartService) : IFavouriteListItems
{
    private readonly IFavouriteListItemRepository _favouriteListRepo = favouriteListRepo;
    private readonly CartService _cartService = cartService;

    public async Task<List<Guid>> ResolveTargetIdsAsync(AddToCartFromFavouritesCommand request, object? favouriteItemsSpecification, CancellationToken cancellationToken = default)
    {
        if (request.isAll)
        {
            if (favouriteItemsSpecification is not Specification<FavouriteListItemEntity> spec)
                throw new BadRequestException("Favourite items specification is required when all=true.");

            var ids = (await _favouriteListRepo.GetManyBySpecAsync(spec, cancellationToken))
                .Select(x => x.InventoryItemId)
                .Distinct()
                .ToList();

            return ids;
        }

        if (request.InventoryItemId is Guid id && id != Guid.Empty)
            return new List<Guid> { id };

        throw new BadRequestException("inventoryItemId is required when all=false.");
    }

    public Task<CartEntity> AddManyToCartAsync(Guid accountId, IEnumerable<Guid> inventoryItemIds, CancellationToken cancellationToken = default)
    {
        return _cartService.AddManyItemsForUserAsync(accountId, inventoryItemIds, cancellationToken);
    }
}
