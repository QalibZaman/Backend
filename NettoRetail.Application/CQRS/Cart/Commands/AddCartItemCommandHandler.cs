using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.CQRS.Cart.Commands;

public record AddItemToCartCommand(
    long ProductUpc,
    string ProductName,
    decimal Price,
    int Quantity,
    decimal? PromotionPrice,
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
        var cartItem = new CartItemEntity
        {
            ProductName = request.ProductName,
            ProductUpc = request.ProductUpc,
            Price = request.Price,
            Quantity = request.Quantity,
            PromotionPrice = request.PromotionPrice,
            Size = null
        };

        try
        {
            var cart = await _cartService.AddItemForUserAsync(request.CustomerId, cartItem, cancellationToken);

            var addedItem = cart.Items.First(i => i.ProductUpc == request.ProductUpc && i.Size == cartItem.Size);

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
        catch (Exception ex)
        {
            var isBusiness =
                ex.Message.Contains("out of stock", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Invalid quantity", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Price mismatch", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Promotion price cannot exceed", StringComparison.OrdinalIgnoreCase);

            return new AddToCartResponse
            {
                Success = false,
                Message = isBusiness ? ex.Message : "Something went wrong. Please try again later.",
                TotalQuantity = 0,
                AddedItem = null
            };
        }
    }
}
