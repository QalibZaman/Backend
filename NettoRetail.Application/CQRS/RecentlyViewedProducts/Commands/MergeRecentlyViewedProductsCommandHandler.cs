using MediatR;
using NettoRetail.Application.Models.Request.RecentlyViewedProduct;
using NettoRetail.Application.Services.IRecentlyViewedService;

namespace NettoRetail.Application.CQRS.RecentlyViewedProducts.Commands;
public record MergeRecentlyViewedProductsCommand(Guid AccountId, IEnumerable<MergeRecentlyViewedItem> Items) : IRequest;
public class MergeRecentlyViewedProductsCommandHandler(IRecentlyViewedService service)
        : IRequestHandler<MergeRecentlyViewedProductsCommand>
{
    private readonly IRecentlyViewedService _service = service;

    public async Task Handle(MergeRecentlyViewedProductsCommand request, CancellationToken cancellationToken)
    {
        await _service.MergeGuestItemsAsync(request.AccountId, request.Items, cancellationToken);
    }
}
