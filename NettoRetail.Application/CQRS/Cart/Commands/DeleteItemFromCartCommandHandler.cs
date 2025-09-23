using MediatR;
using NettoRetail.Application.Services.ICartService;

namespace NettoRetail.Application.CQRS.Cart.Commands;

public record DeleteItemFromCartCommand(Guid InventoryItemId, Guid CustomerId) : IRequest;

public class DeleteItemFromCartCommandHandler(ICartService cartService) : IRequestHandler<DeleteItemFromCartCommand>
{
    public async Task Handle(DeleteItemFromCartCommand request, CancellationToken cancellationToken)
    {
        await cartService.DeleteCartItemAsync(request.CustomerId, request.InventoryItemId, cancellationToken);
    }
}
