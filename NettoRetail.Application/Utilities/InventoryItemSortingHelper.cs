using System.Linq.Expressions;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Utilities;
public static class InventoryItemSortingHelper
{
    public static Expression<Func<InventoryItemEntity, object>> GetSortingExpression(string fieldName) =>
        fieldName.ToLower() switch
        {
            "productname" => item => item.ProductName,
            "brandname" => item => item.Brand!.Name,
            "price" => item => item.Price,
            "availableamount" => item => item.AvailableAmount,
            "reservedamount" => item => item.ReservedAmount,
            "onhandamount" => item => item.OnHandAmount,
            _ => item => item.ProductName
        };
}
