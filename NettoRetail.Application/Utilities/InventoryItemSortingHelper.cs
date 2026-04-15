using System.Linq.Expressions;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Utilities;
public static class InventoryItemSortingHelper
{
    public static Expression<Func<InventoryItemEntity, object>> GetSortingExpression(string fieldName) =>
        fieldName.ToLower() switch
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
            _ => throw new ArgumentException($"Unknown sorting field: {fieldName}", nameof(fieldName))
        };
}
