using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemsByListOfBrandsPartialNameSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByListOfBrandsPartialNameSpecification(List<string> partialNames)
    : base(x => partialNames.Any(v => EF.Functions.ILike(x.Brand == null ? "" : x.Brand.Name, "%" + v + "%"))) { }
}
