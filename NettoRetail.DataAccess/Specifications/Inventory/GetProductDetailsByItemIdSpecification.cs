using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetProductDetailsByItemIdSpecification : Specification<ProductDetailEntity>
{
    public GetProductDetailsByItemIdSpecification(Guid inventoryItemId)
        : base(d => d.InventoryItemId == inventoryItemId) { }
}
