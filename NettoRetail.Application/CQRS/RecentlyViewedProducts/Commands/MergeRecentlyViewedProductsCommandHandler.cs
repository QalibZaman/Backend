using MediatR;
using NettoRetail.Application.Models.Request.RecentlyViewedProduct;
using NettoRetail.Application.Services.IRecentlyViewedService;

<<<<<<< HEAD
namespace NettoRetail.Application.CQRS.RecentlyViewedProducts.Commands;
public record MergeRecentlyViewedProductsCommand(Guid AccountId, IEnumerable<MergeRecentlyViewedItem> Items) : IRequest;
public class MergeRecentlyViewedProductsCommandHandler(IRecentlyViewedService service)
        : IRequestHandler<MergeRecentlyViewedProductsCommand>
{
    private readonly IRecentlyViewedService _service = service;

    public async Task Handle(MergeRecentlyViewedProductsCommand request, CancellationToken cancellationToken)
    {
        await _service.MergeGuestItemsAsync(request.AccountId, request.Items, cancellationToken);
=======
namespace NettoRetail.Application.CQRS.RecentlyViewedProducts.Commands
{
    public record MergeRecentlyViewedProductsCommand(Guid AccountId, IEnumerable<MergeRecentlyViewedItem> Items) : IRequest;
    public class MergeRecentlyViewedProductsCommandHandler
        : IRequestHandler<MergeRecentlyViewedProductsCommand>
    {
        private readonly IRecentlyViewedService _service;

        public MergeRecentlyViewedProductsCommandHandler(IRecentlyViewedService service)
        {
            _service = service;
        }

        public async Task Handle(MergeRecentlyViewedProductsCommand request, CancellationToken cancellationToken)
        {
            await _service.MergeGuestItemsAsync(request.AccountId, request.Items, cancellationToken);
        }
>>>>>>> b1f8ccc (Implement service, handlers and controller)
    }
}
