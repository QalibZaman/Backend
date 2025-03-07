using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;
public class GetProductByUpcSpecification : Specification<InventoryItemEntity>
{
    public GetProductByUpcSpecification(long productUpc)
    : base(x => x.ProductUpc == productUpc) { }
}