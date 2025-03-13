using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;

public class GetInventoryItemsByPartialNameSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByPartialNameSpecification(string? partialName)
    : base(x => x.ProductName.IndexOf(partialName ?? "", StringComparison.OrdinalIgnoreCase) >= 0) { }
    //.Where(u => EF.Functions.Like(u.Name, $"%{searchTerm}%"))
}
