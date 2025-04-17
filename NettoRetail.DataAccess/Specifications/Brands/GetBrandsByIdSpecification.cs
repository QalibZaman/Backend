using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Brands;
public class GetBrandsByIdSpecification : Specification<BrandEntity>
{
    public GetBrandsByIdSpecification(int id)
        : base(model => model.Id == id) { }
}
