using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetCategoriesByPartialNameSpecification : Specification<CategoryEntity>
{
    public GetCategoriesByPartialNameSpecification(string partialName)
        : base(x => EF.Functions.ILike(x.Name, $"%{partialName}%")) { }
}