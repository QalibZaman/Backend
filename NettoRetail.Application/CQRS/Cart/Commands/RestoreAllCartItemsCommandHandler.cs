using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICartService;

namespace NettoRetail.Application.CQRS.Cart.Commands;

public record RestoreAllCartItemsCommand(Guid CustomerId) : IRequest<IEnumerable<CartItemResponse>>;

public class RestoreAllCartItemsCommandHandler(ICartService cartService) : IRequestHandler<RestoreAllCartItemsCommand, IEnumerable<CartItemResponse>>
{
    public async Task<IEnumerable<CartItemResponse>> Handle(RestoreAllCartItemsCommand request, CancellationToken cancellationToken)
    {
        var undoCartItem = await cartService.RestoreAllCartItemsAsync(request.CustomerId, cancellationToken);

        var response = undoCartItem.Adapt<IEnumerable<CartItemResponse>>();

        return response;
    }
}
