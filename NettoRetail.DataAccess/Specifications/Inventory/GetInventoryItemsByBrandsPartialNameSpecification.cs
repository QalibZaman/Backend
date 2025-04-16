using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemsByBrandsPartialNameSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByBrandsPartialNameSpecification(string partialName)
    : base(x => EF.Functions.ILike(x.Brand == null ? "" : x.Brand.Name, $"%{partialName}%")) { }
}
