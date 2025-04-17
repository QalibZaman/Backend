using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Brands;
public class GetBrandsByIdShopIdSpecification
{
    public Specification<BrandEntity> ByIdAndShopId(int id, int shopId)
    {
        return new SpecificationBuilder<BrandEntity>()
            .Add<GetBrandsByIdSpecification>(id)
            .Add<GetBrandsByShopIdSpecification>(shopId)
            .Build();
    }
}
