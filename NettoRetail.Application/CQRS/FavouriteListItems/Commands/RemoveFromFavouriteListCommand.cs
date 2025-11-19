using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.FavouriteListItems.Commands;

public record RemoveFromFavouriteListCommand(Guid UserId, Guid InventoryItemId) : IRequest;

public class RemoveFromFavouriteListCommandHandler(
    IFavouriteListItemRepository favouriteListItemRepository
) : IRequestHandler<RemoveFromFavouriteListCommand>
{
    public async Task Handle(RemoveFromFavouriteListCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetFavouriteListItemByAccountAndItemId(request.UserId, request.InventoryItemId);

        var favouriteItem = await favouriteListItemRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Item is not in your favourites");

        await favouriteListItemRepository.DeleteAsync(favouriteItem, cancellationToken);
    }
}