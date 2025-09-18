using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICartService;

namespace NettoRetail.Application.CQRS.Cart.Commands;

public record UpdateCartItemQuantityCommand(
        Guid CartItemId,
        int Quantity,
        Guid CustomerId) : IRequest<CartItemResponse>;
public class UpdateCartItemQuantityCommandHandler(ICartService cartService) : IRequestHandler<UpdateCartItemQuantityCommand, CartItemResponse>
{
    private readonly ICartService _cartService = cartService;

    public async Task<CartItemResponse> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var updatedItem = await _cartService.UpdateItemQuantityAsync(
                    request.CustomerId,
                    request.CartItemId,
                    request.Quantity,
                    cancellationToken);

        return new CartItemResponse
        {
            ProductUpc = updatedItem.ProductUpc,
            ProductName = updatedItem.ProductName,
            Price = updatedItem.Price,
            Quantity = updatedItem.Quantity,
            Size = updatedItem.Size,
            PromotionPrice = updatedItem.PromotionPrice
        };
    }
}
