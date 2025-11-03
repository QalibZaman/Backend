using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.FavouriteListItems.Commands;

public record AddToCartFromFavouritesCommand(
    Guid AccountId,
    int ShopId,
    Guid? InventoryItemId,
    bool All) : IRequest<AddToCartResponse>;

public class AddToCartFromFavouritesCommandHandler(
    IFavouriteListItemRepository favouriteRepo,
    ICartService cartService)
    : IRequestHandler<AddToCartFromFavouritesCommand, AddToCartResponse>
{
    public async Task<AddToCartResponse> Handle(AddToCartFromFavouritesCommand request, CancellationToken ct)
    {
        List<Guid> targetIds;

        if (request.All)
        {
            targetIds = (await favouriteRepo.GetManyBySpecAsync(
                    new GetFavouriteListByAccountAndShop(request.AccountId, request.ShopId), ct))
                .Select(f => f.InventoryItemId)
                .Distinct()
                .ToList();
        }
        else if (request.InventoryItemId is Guid id && id != Guid.Empty)
        {
            targetIds = new() { id };
        }
        else
        {
            throw new BadRequestException("inventoryItemId is required when all=false.");
        }

        if (targetIds.Count == 0)
            throw new NotFoundException("No favourite items found to add.");

        var addedCount = 0;
        CartEntity? lastCart = null;
        CartItemEntity? lastLine = null;

        foreach (var id in targetIds)
        {
            try
            {
                lastCart = await cartService.AddItemForUserAsync(request.AccountId, id, ct);
                lastLine = lastCart.Items.FirstOrDefault(i => i.InventoryItemId == id) ?? lastLine;
                addedCount++;
            }
            catch when (!request.All)
            {
                throw new BadRequestException("Item could not be added (unavailable or at maximum quantity).");
            }
        }

        if (request.All && addedCount == 0)
            throw new BadRequestException("No items were added (all unavailable or at maximum quantity).");

        var totalQty = lastCart?.Items.Sum(i => i.Quantity) ?? 0;
        var addedItem = lastLine?.Adapt<CartItemResponse>();
        var message = (addedCount == 1 && !request.All) ? "Item added to cart." : $"{addedCount} items added to cart.";

        return new AddToCartResponse
        {
            Success = true,
            Message = message,
            TotalQuantity = totalQty,
            AddedItem = addedItem
        };
    }
}