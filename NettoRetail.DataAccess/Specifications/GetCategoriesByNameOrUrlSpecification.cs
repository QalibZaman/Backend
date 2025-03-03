using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;

public class GetCategoriesByNameOrUrlSpecification : Specification<CategoryEntity>
{
    public GetCategoriesByNameOrUrlSpecification(string Name, string URL)
            : base(category => category.Name == Name || category.URL == URL) { }
}
