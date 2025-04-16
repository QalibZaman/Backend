using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Brands;

public class GetBrandsByNameSpecification : Specification<BrandEntity>
{
    public GetBrandsByNameSpecification(string name)
    : base(b => b.Name == name) { }
}
