using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;
public class GetCategoriesOfParentByIdSpecification : Specification<CategoryEntity>
{
    public GetCategoriesOfParentByIdSpecification(int? id)
            : base(category => category.ParentId == id) { }
}