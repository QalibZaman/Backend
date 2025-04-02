using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetProductDetailsByInventoryItemIdSpecification : Specification<ProductDetailsEntity>
{
    public GetProductDetailsByInventoryItemIdSpecification(Guid inventoryItemId)
    : base(x => x.InventoryItemId == inventoryItemId) { }
}
