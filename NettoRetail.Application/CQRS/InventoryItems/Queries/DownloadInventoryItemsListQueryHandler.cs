using System.Globalization;
using System.Text;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record DownloadInventoryItemsListQuery : IRequest<byte[]>;

public class DownloadInventoryItemsListQueryHandler(IInventoryItemRepository inventoryItemRepository, ICsvFileService csvFileService) : IRequestHandler<DownloadInventoryItemsListQuery, byte[]>
{
    public async Task<byte[]> Handle(DownloadInventoryItemsListQuery request, CancellationToken cancellationToken)
    {
        var itemByRelatedCategorySpecification = new GetInventoryItemRelatedCategories();
        var inventoryItems = await inventoryItemRepository.GetAllBySpecAsync(itemByRelatedCategorySpecification, cancellationToken) ?? throw new NotFoundException("There are no items in the list.");

        const string header = "Product UPC,Product Name,Product Location,Product Category,Available Amount,Reserved Amount,On-hand Amount,Price,Status,Description";
        var inventoryItemsListData = csvFileService.GetEntitiesCsvData(inventoryItems, header, item => $"{item.ProductUpc},{item.ProductName},{item.ProductLocation},{item.Category.Name},{item.AvailableAmount},{item.ReservedAmount},{item.OnHandAmount},{item.Price.ToString("0.00", CultureInfo.InvariantCulture)},{item.Status},{item.Description}");

        var encodedInventoryData = Encoding.UTF8.GetBytes(inventoryItemsListData);

        return encodedInventoryData;
    }
}