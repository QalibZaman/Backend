using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetCategoriesByNameOrUrlSpecification : Specification<CategoryEntity>
{
    public GetCategoriesByNameOrUrlSpecification(string? name, string? url, int shopId, int? excludeId = null)
        : base(BuildCriteria(name, url, shopId, excludeId)) { }

    private static Expression<Func<CategoryEntity, bool>> BuildCriteria(
        string? name,
        string? url,
        int shopId,
        int? excludeId)
    {
        var normalizedName = string.IsNullOrWhiteSpace(name)
            ? null
            : name.Trim().ToLower();

        var normalizedUrl = string.IsNullOrWhiteSpace(url)
            ? null
            : url.Trim().ToLower();

        return category =>
            category.ShopId == shopId
            && (excludeId == null || category.Id != excludeId)
            && (
                (normalizedName != null && category.Name.ToLower() == normalizedName)
                || (normalizedUrl != null && category.URL.ToLower() == normalizedUrl)
            );
    }
}
