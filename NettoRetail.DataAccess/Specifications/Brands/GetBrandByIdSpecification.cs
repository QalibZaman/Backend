using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Brands;

public class GetBrandByIdSpecification : Specification<BrandEntity>
{
    public GetBrandByIdSpecification(Guid id)
    : base(b => b.Id == id) { }
}
