using MediatR;
using NettoRetail.Application.CsvFileMap;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record DownloadInventoryItemsListQuery : IRequest<byte[]>;

public class DownloadInventoryItemsListQueryHandler(IInventoryItemRepository inventoryItemRepository, ICsvFileService csvFileService) : IRequestHandler<DownloadInventoryItemsListQuery, byte[]>
{
    public async Task<byte[]> Handle(DownloadInventoryItemsListQuery request, CancellationToken cancellationToken)
    {
        var itemByRelatedCategorySpecification = new GetInventoryItemRelatedCategories();
        var inventoryItems = await inventoryItemRepository.GetAllBySpecAsync(itemByRelatedCategorySpecification, cancellationToken) ?? throw new NotFoundException("There are no items in the list.");

        var inventoryItemsData = await csvFileService.GetEntitiesCsvData<InventoryItemEntity, InventoryItemMap>(inventoryItems, cancellationToken);

        return inventoryItemsData;
    }
}