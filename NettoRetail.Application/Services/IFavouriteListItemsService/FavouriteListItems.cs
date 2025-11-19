using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.FavouriteListItems;

public class FavouriteListItemsService(
    IFavouriteListItemRepository favouriteListRepo,
    ICartService.ICartService cartService) : IFavouriteListItemsService
{
    public async Task<List<Guid>> ResolveTargetIdsAsync(
        AddToCartFromFavouritesCommand request,
        Specification<FavouriteListItemEntity> favouriteItemsSpecification,
        CancellationToken cancellationToken = default)
    {
        if (request.isAll)
        {
            if (favouriteItemsSpecification is null)
                throw new BadRequestException("Favourite items specification is required when all=true.");

            var ids = (await favouriteListRepo.GetManyBySpecAsync(favouriteItemsSpecification, cancellationToken))
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
        => cartService.AddManyItemsForUserAsync(accountId, inventoryItemIds, cancellationToken);
}