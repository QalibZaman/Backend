using MediatR;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record DownloadInventoryItemsListQuery : IRequest<>;

public class DownloadInventoryItemsListQueryHandler(IInventoryItemRepository inventoryItemRepository) : IRequestHandler<DownloadInventoryItemsListQuery, >
{
    public async Task<> Handle(CancellationToken cancellationToken)
    {
        var inventoryItems = await inventoryItemRepository.GetAllAsync(cancellationToken);
        

        return ;
    }
}