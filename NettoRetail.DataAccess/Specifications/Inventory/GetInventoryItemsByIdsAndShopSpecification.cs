using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

/// <summary>
/// Filters inventory items by a set of ids within a shop.
/// Includes are added by the handler via SpecificationBuilder.
/// </summary>
public sealed class GetInventoryItemsByIdsAndShopSpecification(
    IReadOnlyCollection<Guid> ids,
    int shopId) : Specification<InventoryItemEntity>(x => ids.Contains(x.Id) && x.ShopId == shopId) { }
