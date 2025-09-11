using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.FavouriteList;

public class GetFavouriteListItemByAccountAndItemId : Specification<FavouriteListItemEntity>
{
    public GetFavouriteListItemByAccountAndItemId(Guid accountId, Guid inventoryItemId)
    : base(x => x.AccountId == accountId && x.InventoryItemId == inventoryItemId) { }
}
