using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;

public class GetInventoryItemsByPartialLocationSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByPartialLocationSpecification(string? partialLocation)
    : base(x => EF.Functions.ILike(x.ProductLocation, $"%{partialLocation}%")) { }
}
