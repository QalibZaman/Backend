using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.Application.Services.IFavouriteListItemsService;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.FavouriteListItems.Commands;

public record AddToCartFromFavouritesCommand(
    Guid AccountId,
    int ShopId,
    Guid? InventoryItemId,
    bool isAll) : IRequest<AddToCartResponse>;

public class AddToCartFromFavouritesCommandHandler(
    IFavouriteListItemRepository favouriteRepo,
    ICartService cartService,
    IFavouriteListItems favouriteListItems)
    : IRequestHandler<AddToCartFromFavouritesCommand, AddToCartResponse>
{
    private const int ParallelAddThreshold = 50;        // start concurrency at this many items
    private const int MaxDegreeOfParallelism = 8;       // upper bound for concurrent adds

    public async Task<AddToCartResponse> Handle(AddToCartFromFavouritesCommand request, CancellationToken cancellationToken)
    {
        var favouritesSpec = request.isAll
            ? new GetFavouriteListByAccountAndShop(request.AccountId, request.ShopId)
            : null;

        var targetIds = await favouriteListItems.ResolveTargetIdsAsync(request,favouritesSpec,cancellationToken);

        if (targetIds.Count == 0)
            throw new NotFoundException("No favourite items found to add.");

        CartEntity? lastCart = null;
        CartItemEntity? lastLine = null;
        int addedCount;

        if (request.isAll)
        {
            var degreeOfParallelism = targetIds.Count >= ParallelAddThreshold ? MaxDegreeOfParallelism : 1;
            var result = await favouriteListItems.AddManyToCartAsync(request.AccountId, targetIds, degreeOfParallelism, cancellationToken);
            if (result.AddedCount == 0) throw new BadRequestException("No items were added (all unavailable or at maximum quantity).");

            addedCount = result.AddedCount;
            lastCart = result.LastCart;
            lastLine = result.LastLine;
        }
        else
        {
            try
            {
                lastCart = await cartService.AddItemForUserAsync(request.AccountId, targetIds[0], cancellationToken);
                lastLine = lastCart.Items.FirstOrDefault(i => i.InventoryItemId == targetIds[0]);
                addedCount = 1;
            }
            catch
            {
                throw new BadRequestException("Item could not be added.");
            }
        }

        return new AddToCartResponse
        {
            Success = true,
            Message = request.isAll ? $"{addedCount} item(s) added to cart." : "Item added to cart.",
            TotalQuantity = lastCart?.Items.Sum(i => i.Quantity) ?? 0,
            AddedItem = lastLine?.Adapt<CartItemResponse>()
        };
    }
}
