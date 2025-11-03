using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.FavouriteList;

public sealed class GetFavouriteListByAccountAndShop(Guid accountId, int shopId)
    : Specification<FavouriteListItemEntity>(x => x.AccountId == accountId
         && x.InventoryItem != null
         && x.InventoryItem.ShopId == shopId)
{ }
