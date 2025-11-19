using MediatR;
using NettoRetail.Application.Services.IInventoryService;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record DownloadInventoryItemsListQuery(int UserShopId) : IRequest<byte[]>;

public class DownloadInventoryItemsListQueryHandler(IInventoryService inventoryService) : IRequestHandler<DownloadInventoryItemsListQuery, byte[]>
{
    public async Task<byte[]> Handle(DownloadInventoryItemsListQuery request, CancellationToken cancellationToken)
    {
        var response = await inventoryService.DownloadInventoryItemsCsvAsync(request.UserShopId, cancellationToken);
        return response;
    }
}