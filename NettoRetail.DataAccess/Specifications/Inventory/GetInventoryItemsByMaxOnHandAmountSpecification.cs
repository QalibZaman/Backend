using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemsByMaxOnHandAmountSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByMaxOnHandAmountSpecification(int maxAmount)
        : base(x => x.OnHandAmount <= maxAmount) { }
}