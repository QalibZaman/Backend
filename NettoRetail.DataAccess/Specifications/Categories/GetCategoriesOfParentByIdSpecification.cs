using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetCategoriesOfParentByIdSpecification(int parentId) : Specification<CategoryEntity>(category => category.ParentId == parentId)
{
}