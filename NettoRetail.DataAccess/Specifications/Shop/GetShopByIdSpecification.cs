using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Shop;

public class GetShopByIdSpecification : Specification<ShopEntity>
{
    public GetShopByIdSpecification(int id)
        : base(shop => shop.Id == id)
    {
    }
}