using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;
public class GetInventoryItemsByMaxAvailableAmountSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByMaxAvailableAmountSpecification(int maxAmount)
        : base(x => x.AvailableAmount <= maxAmount) { }
}