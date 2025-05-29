using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NettoRetail.DataAccess.Specifications.Content;
public class GetContentByShopSpecification : Specification<ContentEntity>
{
    public GetContentByShopSpecification(int shopId)
        : base(c => c.ShopId == shopId)
    {
    }
}