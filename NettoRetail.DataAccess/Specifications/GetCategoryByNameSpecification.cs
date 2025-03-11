using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;

public class GetCategoryByNameSpecification : Specification<CategoryEntity>
{
    public GetCategoryByNameSpecification(string categoryName)
            : base(category => category.Name == categoryName) { }
}
