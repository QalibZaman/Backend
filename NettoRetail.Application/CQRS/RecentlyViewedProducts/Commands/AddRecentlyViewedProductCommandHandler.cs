using MediatR;
using NettoRetail.Application.Services.IRecentlyViewedService;

namespace NettoRetail.Application.CQRS.RecentlyViewedProducts.Commands;
public record AddRecentlyViewedProductCommand(Guid AccountId, Guid InventoryItemId) : IRequest;

public class AddRecentlyViewedProductCommandHandler(IRecentlyViewedService service)
                    : IRequestHandler<AddRecentlyViewedProductCommand>
{
    private readonly IRecentlyViewedService _service = service;

    public async Task Handle(AddRecentlyViewedProductCommand request, CancellationToken cancellationToken)
    {
        await _service.AddOrUpdateAsync(request.AccountId, request.InventoryItemId, cancellationToken);
    }
}
