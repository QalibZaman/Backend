using MediatR;
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
        await favouriteListItemRepository.DeleteManyBySpecAsync(spec, cancellationToken);
    }
}