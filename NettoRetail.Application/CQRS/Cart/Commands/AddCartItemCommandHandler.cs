using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.CQRS.Cart.Commands;

public record AddItemToCartCommand(
    Guid InventoryItemId,
    Guid CustomerId
) : IRequest<AddToCartResponse>;

public class AddItemToCartCommandHandler
    : IRequestHandler<AddItemToCartCommand, AddToCartResponse>
{
    private readonly ICartService _cartService;

    public AddItemToCartCommandHandler(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<AddToCartResponse> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartService.AddItemForUserAsync(
            request.CustomerId,
            request.InventoryItemId,
            cancellationToken
        );

        var addedItem = cart.Items.First(i => i.InventoryItemId == request.InventoryItemId);

        return new AddToCartResponse
        {
            Success = true,
            Message = "Added to bag",
            TotalQuantity = cart.Items.Sum(i => i.Quantity),
            AddedItem = new CartItemResponse
            {
                ProductUpc = addedItem.ProductUpc,
                ProductName = addedItem.ProductName,
                Price = addedItem.Price,
                Quantity = addedItem.Quantity,
                Size = addedItem.Size,
                PromotionPrice = addedItem.PromotionPrice
            }
        };
    }
}
