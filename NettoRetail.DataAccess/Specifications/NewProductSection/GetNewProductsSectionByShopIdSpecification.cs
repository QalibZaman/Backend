using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.NewProductSection;

public class GetNewProductsSectionByShopIdSpecification : Specification<NewProductsSectionEntity>
{
    public GetNewProductsSectionByShopIdSpecification(int shopId)
        : base(x => x.ShopId == shopId)
    {
        AddInclude(x => x.Products);
    }
}