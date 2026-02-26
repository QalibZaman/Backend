using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;
public class GetProductByUpcSpecification : Specification<InventoryItemEntity>
{
    public GetProductByUpcSpecification(string productUpc)
    : base(x => x.ProductUpc == productUpc) { }
}
