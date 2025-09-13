using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.FavouriteListItems.Commands;

public record AddToFavouriteListCommand(Guid UserId, Guid InventoryItemId) : IRequest<AddToFavouritesResponse>;
public class AddToFavouriteListCommandHandler(
        IFavouriteListItemRepository favouriteListItemRepository,
        IInventoryItemRepository inventoryItemRepository,
        IAccountRepository accountRepository) : IRequestHandler<AddToFavouriteListCommand, AddToFavouritesResponse>
{
    public async Task<AddToFavouritesResponse> Handle(AddToFavouriteListCommand request, CancellationToken cancellationToken)
    {
        var inventoryItemSpec = new GetInventoryItemByIdSpecification(request.InventoryItemId);
        var inventoryItemExists = await inventoryItemRepository.Exists(inventoryItemSpec, cancellationToken);
        if (!inventoryItemExists)
        {
            throw new NotFoundException($"No inventory item with Id: {request.InventoryItemId}");
        }

        var accountSpec = new GetAccountByIdSpecification(request.UserId);
        var accountExists = await accountRepository.Exists(accountSpec, cancellationToken);
        if (!accountExists)
        {
            throw new NotFoundException($"No account with Id: {request.UserId}");
        }

        var favouriteListItemSpec = new GetFavouriteListItemByAccountAndItemId(request.UserId, request.InventoryItemId);
        var favouriteItemExists = await favouriteListItemRepository.Exists(favouriteListItemSpec, cancellationToken);
        if (favouriteItemExists)
        {
            throw new BadRequestException("Item is already in favourites");
        }

        var favouriteListItem = new FavouriteListItemEntity
        {
            AccountId = request.UserId,
            InventoryItemId = request.InventoryItemId
        };

        var createdItem = await favouriteListItemRepository.AddAsync(favouriteListItem, cancellationToken);

        return createdItem.Adapt<AddToFavouritesResponse>();
    }
}