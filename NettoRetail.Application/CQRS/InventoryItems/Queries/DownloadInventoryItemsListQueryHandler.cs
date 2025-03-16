using System.Text;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record DownloadInventoryItemsListQuery : IRequest<byte[]>;

public class DownloadInventoryItemsListQueryHandler(IInventoryItemRepository inventoryItemRepository) : IRequestHandler<DownloadInventoryItemsListQuery, byte[]>
{
    public async Task<byte[]> Handle(DownloadInventoryItemsListQuery request, CancellationToken cancellationToken)
    {
        var itemByRelatedCategorySpecification = new GetInventoryItemRelatedCategories();
        var inventoryItems = await inventoryItemRepository.GetAllBySpecAsync(itemByRelatedCategorySpecification, cancellationToken) ?? throw new NotFoundException("There are no items in the list.");
        
        var inventoryItemsListData = GetInventoryItemsListData(inventoryItems);

        var encodedInventoryData = Encoding.UTF8.GetBytes(inventoryItemsListData);

        return encodedInventoryData;
    }

    private string GetInventoryItemsListData(IEnumerable<InventoryItemEntity> inventoryItems)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("Product UPC,Product Name,Product Location,Product Category,Available Amount,Reserved Amount,On-hand Amount,Price,Status,Description");

        foreach (var item in inventoryItems)
        {
            stringBuilder.AppendLine($"{item.ProductUpc},{item.ProductName},{item.ProductLocation},{item.Category.Name},{item.AvailableAmount},{item.ReservedAmount},{item.OnHandAmount},{item.Price},{item.Status},{item.Description}");
        }

        return stringBuilder.ToString();
    }
}