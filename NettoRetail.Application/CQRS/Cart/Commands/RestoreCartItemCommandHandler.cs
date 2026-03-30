using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICartService;

namespace NettoRetail.Application.CQRS.Cart.Commands;

public record RestoreCartItemCommand(Guid InventoryItemId, Guid CustomerId) : IRequest<CartItemResponse>;

public class RestoreCartItemCommandHandler(ICartService cartService, IMapper mapper) : IRequestHandler<RestoreCartItemCommand, CartItemResponse>
{
    public async Task<CartItemResponse> Handle(RestoreCartItemCommand request, CancellationToken cancellationToken)
    {
        var undoCartItem = await cartService.RestoreCartItemAsync(request.CustomerId, request.InventoryItemId, cancellationToken);

        var response = mapper.Map<CartItemResponse>(undoCartItem);

        return response;
    }
}
