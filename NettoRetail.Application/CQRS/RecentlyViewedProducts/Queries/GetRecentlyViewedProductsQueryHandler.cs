using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IRecentlyViewedService;

namespace NettoRetail.Application.CQRS.RecentlyViewedProducts.Queries
{
    public record GetRecentlyViewedProductsQuery(Guid AccountId)
        : IRequest<List<InventoryItemCardResponse>>;

    public class GetRecentlyViewedProductsQueryHandler
        : IRequestHandler<GetRecentlyViewedProductsQuery, List<InventoryItemCardResponse>>
    {
        private readonly IRecentlyViewedService _service;

        public GetRecentlyViewedProductsQueryHandler(IRecentlyViewedService service)
        {
            _service = service;
        }

        public async Task<List<InventoryItemCardResponse>> Handle(
            GetRecentlyViewedProductsQuery request,
            CancellationToken cancellationToken)
        {
            return await _service.GetListAsync(request.AccountId, cancellationToken);
        }
    }
}
