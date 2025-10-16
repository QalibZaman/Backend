using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Brands;


public class GetBrandsByPartialNameSpecification : Specification<BrandEntity>
{
    public GetBrandsByPartialNameSpecification(string partialName)
        : base(x => EF.Functions.ILike(x.Name, $"%{partialName}%")) { }
}
