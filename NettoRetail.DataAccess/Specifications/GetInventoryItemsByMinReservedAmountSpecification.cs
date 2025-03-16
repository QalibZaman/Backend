using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;
public class GetInventoryItemsByMinReservedAmountSpecifiaction : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByMinReservedAmountSpecifiaction(int minAmount)
        : base(x => x.ReservedAmount >= minAmount) { }
}