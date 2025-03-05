using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;

public class GetCategoriesByNameOrUrlSpecification : Specification<CategoryEntity>
{
    public GetCategoriesByNameOrUrlSpecification(string name, string url)
            : base(category => category.Name == name || category.URL == url) { }
}
