using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.FavouriteListItems;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.CQRS.FavouriteListItems.Commands;

public record AddToCartFromFavouritesCommand(
    Guid AccountId,
    int ShopId,
    Guid? InventoryItemId,
    bool isAll) : IRequest<AddToCartResponse>;

public class AddToCartFromFavouritesCommandHandler(
    ICartService cartService,
    IFavouriteListItemsService favouriteListItems,
    IMapper mapper)
    : IRequestHandler<AddToCartFromFavouritesCommand, AddToCartResponse>
{
    public async Task<AddToCartResponse> Handle(AddToCartFromFavouritesCommand request, CancellationToken cancellationToken)
    {
        CartEntity cart;
        CartItemEntity? addedLine = null;

        var favouritesSpec = request.isAll ? new GetFavouriteListByAccountAndShop(request.AccountId, request.ShopId) : null;

        var targetIds = await favouriteListItems.ResolveTargetIdsAsync(request, favouritesSpec, cancellationToken);

        if (targetIds.Count == 0)
            throw new NotFoundException("No favourite items found to add.");

        if (request.isAll)
        {
            cart = await favouriteListItems.AddManyToCartAsync(request.AccountId, targetIds, cancellationToken);
        }
        else
        {
            cart = await cartService.AddItemForUserAsync(request.AccountId, targetIds[0], cancellationToken);
            addedLine = cart.Items.FirstOrDefault(i => i.InventoryItemId == targetIds[0]);
        }

        return new AddToCartResponse
        {
            Success = true,
            Message = request.isAll ? "Items added to cart." : "Item added to cart.",
            TotalQuantity = cart.Items.Sum(i => i.Quantity),
            AddedItem = addedLine == null ? null : mapper.Map<CartItemResponse>(addedLine)
        };
    }
}
