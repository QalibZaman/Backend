using MediatR;
using NettoRetail.Application.Services.ICartService;

namespace NettoRetail.Application.CQRS.Cart.Commands;

public record ClearItemsFromCartCommand(Guid CustomerId) : IRequest;

public class ClearItemsFromCartCommandHandler(ICartService cartService) : IRequestHandler<ClearItemsFromCartCommand>
{
    public async Task Handle(ClearItemsFromCartCommand request, CancellationToken cancellationToken)
    {
        await cartService.ClearCartItemsAsync(request.CustomerId, cancellationToken);
    }
}
