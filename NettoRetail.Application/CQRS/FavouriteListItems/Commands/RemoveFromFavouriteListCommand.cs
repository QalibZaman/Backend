using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.FavouriteListItems.Commands;

public record RemoveFromFavouriteListCommand(Guid UserId, Guid InventoryItemId) : IRequest<DeleteItemFromFavouritesResponse>;

public class RemoveFromFavouriteListCommandHandler(
    IFavouriteListItemRepository favouriteListItemRepository
) : IRequestHandler<RemoveFromFavouriteListCommand, DeleteItemFromFavouritesResponse>
{
    public async Task<DeleteItemFromFavouritesResponse> Handle(RemoveFromFavouriteListCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetFavouriteListItemByAccountAndItemId(request.UserId, request.InventoryItemId);

        var favouriteItemExists = await favouriteListItemRepository.Exists(spec, cancellationToken);
        if (!favouriteItemExists)
            throw new NotFoundException("Item is not in your favourites");

        await favouriteListItemRepository.DeleteAsync(spec, cancellationToken);

        var favouriteListSpecification = new GetFavouriteListByAccountIdSpecification(request.UserId);

        var favouriteList = await favouriteListItemRepository.GetAllBySpecAsync(favouriteListSpecification, cancellationToken);

        var response = DeleteItemFromFavouritesResponse.AdaptToResponse(favouriteList);

        return response;
    }
}