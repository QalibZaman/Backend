using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemsByCategoriesSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByCategoriesSpecification(List<int> ids)
        : base(x => ids.Contains(x.CategoryId)) { }
}