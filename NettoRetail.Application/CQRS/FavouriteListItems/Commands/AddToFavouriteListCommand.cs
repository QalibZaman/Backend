using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.FavouriteListItems.Commands;

public record AddToFavouriteListCommand(Guid UserId, Guid InventoryItemId) : IRequest<AddToFavouritesResponse>;
public class AddToFavouriteListCommandHandler(
        IFavouriteListItemRepository favouriteListItemRepository,
        IInventoryItemRepository inventoryItemRepository,
        IStaffRepository accountRepository,
        IMapper mapper) : IRequestHandler<AddToFavouriteListCommand, AddToFavouritesResponse>
{
    public async Task<AddToFavouritesResponse> Handle(AddToFavouriteListCommand request, CancellationToken cancellationToken)
    {
        var inventoryItemSpec = new GetInventoryItemByIdSpecification(request.InventoryItemId);
        var inventoryItemExists = await inventoryItemRepository.Exists(inventoryItemSpec, cancellationToken);
        if (!inventoryItemExists)
        {
            throw new NotFoundException($"No inventory item with Id: {request.InventoryItemId}");
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

        return mapper.Map<AddToFavouritesResponse>(createdItem);
    }
}