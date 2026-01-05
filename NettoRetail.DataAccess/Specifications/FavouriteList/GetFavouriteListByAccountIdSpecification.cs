using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.FavouriteList;

public class GetFavouriteListByAccountIdSpecification : Specification<FavouriteListItemEntity>
{
    public GetFavouriteListByAccountIdSpecification(Guid userId)
        : base(f => f.AccountId == userId)
    {
        AddInclude(f => f.InventoryItem);
    }
}
