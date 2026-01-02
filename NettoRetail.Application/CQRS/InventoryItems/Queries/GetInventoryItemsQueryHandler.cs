using MediatR;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IInventoryItemQueryService;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemsQuery(
    int ShopId,
    InventoryItemsSearchModel Model) : IRequest<PaginatedResponse<InventoryItemListResponse>>;

public class GetInventoryItemsQueryHandler(IInventoryItemQueryService listingService)
        : IRequestHandler<GetInventoryItemsQuery, PaginatedResponse<InventoryItemListResponse>>
{
    private readonly IInventoryItemQueryService _queryService = listingService;

    public async Task<PaginatedResponse<InventoryItemListResponse>> Handle(
        GetInventoryItemsQuery request,
        CancellationToken cancellationToken)
    {
        return await _queryService.GetInventoryItemsAsync(
            request.ShopId,
            request.Model,
            cancellationToken);
    }
}
