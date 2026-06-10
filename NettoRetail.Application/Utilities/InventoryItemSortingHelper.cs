using System.Linq.Expressions;
using NettoRetail.Application.Exceptions;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Utilities;
public static class InventoryItemSortingHelper
{
    public static Expression<Func<InventoryItemEntity, object>> GetSortingExpression(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return item => item.Id;

        return fieldName.Trim().ToLowerInvariant() switch
        {
            "productname" => item => item.ProductName,
            "productupc" => item => item.ProductUpc,
            "productlocation" => item => item.ProductLocation,
            "category" => item => item.Category.Name,
            "brandname" => item => item.Brand!.Name,
            "price" => item => item.Price,
            "availableamount" => item => item.AvailableAmount,
            "reservedamount" => item => item.ReservedAmount,
            "onhandamount" => item => item.OnHandAmount,
            "status" => item => item.Status,
            "lastupdateddate" => item => item.LastUpdatedDate,
            _ => item => item.Id
        };
    }
}
