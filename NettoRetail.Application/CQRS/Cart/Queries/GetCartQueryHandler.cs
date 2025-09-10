using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICartService;

public record GetCartQuery(Guid CustomerId) : IRequest<List<CartItemResponse>>;

public class GetCartQueryHandler
    : IRequestHandler<GetCartQuery, List<CartItemResponse>>
{
    private readonly ICartService _cartService;

    public GetCartQueryHandler(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<List<CartItemResponse>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cart = await _cartService.GetCartAsync(request.CustomerId, cancellationToken);

            return cart.Items
                       .Select(i => new CartItemResponse
                       {
                           ProductUpc = i.ProductUpc,
                           ProductName = i.ProductName,
                           Price = i.Price,
                           Quantity = i.Quantity,
                           Size = i.Size,
                           PromotionPrice = i.PromotionPrice
                       })
                       .ToList();
        }
        catch (NotFoundException)
        {
            return new List<CartItemResponse>();
        }
    }
}
