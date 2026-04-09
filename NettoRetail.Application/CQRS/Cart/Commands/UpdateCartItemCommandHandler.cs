using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICartService;

namespace NettoRetail.Application.CQRS.Cart.Commands;

public record UpdateCartItemQuantityCommand(
        Guid CartItemId,
        int Quantity,
        Guid CustomerId) : IRequest<CartItemResponse>;
public class UpdateCartItemQuantityCommandHandler(ICartService cartService, IMapper mapper) : IRequestHandler<UpdateCartItemQuantityCommand, CartItemResponse>
{
    private readonly ICartService _cartService = cartService;
    private readonly IMapper _mapper = mapper;

    public async Task<CartItemResponse> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var updatedItem = await _cartService.UpdateItemQuantityAsync(
                    request.CustomerId,
                    request.CartItemId,
                    request.Quantity,
                    cancellationToken);

        var response = _mapper.Map<CartItemResponse>(updatedItem);

        return response;
    }
}
