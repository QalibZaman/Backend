using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.FavouriteListItems.Commands;

public record RemoveFromFavouriteListCommand(Guid UserId, Guid InventoryItemId) : IRequest;
public class RemoveFromFavouriteListCommandHandler(
        IFavouriteListItemRepository favouriteListItemRepository,
        IAccountRepository accountRepository) : IRequestHandler<RemoveFromFavouriteListCommand>
{
    public async Task Handle(RemoveFromFavouriteListCommand request, CancellationToken cancellationToken)
    {
        var accountSpec = new GetAccountByIdSpecification(request.UserId);
        var accountExists = await accountRepository.Exists(accountSpec, cancellationToken);
        if (!accountExists)
        {
            throw new NotFoundException($"No account with Id: {request.UserId}");
        }

        var inventoryItemSpec = new GetFavouriteListItemByAccountAndItemId(request.UserId, request.InventoryItemId);
        var favouriteListItem = await favouriteListItemRepository.GetAsync(inventoryItemSpec, cancellationToken) ?? throw new NotFoundException("Favourite item not found");

        await favouriteListItemRepository.DeleteAsync(favouriteListItem, cancellationToken);
    }
}
