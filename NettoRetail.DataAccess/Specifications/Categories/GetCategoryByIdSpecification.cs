using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetCategoryByIdSpecification : Specification<CategoryEntity>
{
    public GetCategoryByIdSpecification(int? id)
            : base(category => category.Id == id) { }
}
