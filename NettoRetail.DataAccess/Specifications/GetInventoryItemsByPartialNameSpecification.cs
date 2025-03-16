using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;

public class GetInventoryItemsByPartialNameSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByPartialNameSpecification(string? partialName)
    : base(x => EF.Functions.ILike(x.ProductName, $"%{partialName}%")) { }
}
