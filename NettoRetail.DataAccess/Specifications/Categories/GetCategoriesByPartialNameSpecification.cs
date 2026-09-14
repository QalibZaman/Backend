using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetCategoriesByPartialNameSpecification(string partialName) : Specification<CategoryEntity>(x => EF.Functions.ILike(x.Name, $"%{partialName}%"))
{
}