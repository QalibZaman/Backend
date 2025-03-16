using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;
public class GetInventoryItemsByMaxReservedAmountSpecifiaction: Specification<InventoryItemEntity>
{
    public GetInventoryItemsByMaxReservedAmountSpecifiaction(int maxAmount)
        : base(x => x.ReservedAmount <= maxAmount) { }
}