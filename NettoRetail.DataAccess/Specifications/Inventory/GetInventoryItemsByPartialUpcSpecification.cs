using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemsByPartialUpcSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByPartialUpcSpecification(string upc)
    : base(x => EF.Functions.ILike(x.ProductUpc, $"%{upc}%")) { }
}
