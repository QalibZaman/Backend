using System.Text;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.ICategoryService;

public class CategoryUrlService(ICategoryRepository categoryRepository) : ICategoryUrlService
{
    public async Task<string> GenerateUniqueUrlAsync(string categoryName, int shopId, CancellationToken cancellationToken)
    {
        var baseSlug = BuildSlug(categoryName);
        var candidateUrl = $"/{baseSlug}";
        var suffix = 1;

        while (await categoryRepository.ExistsAsync(
            new GetCategoryByUrlSpecification(candidateUrl, shopId),
            cancellationToken))
        {
            suffix++;
            candidateUrl = $"/{baseSlug}-{suffix}";
        }

        return candidateUrl;
    }

    private static string BuildSlug(string name)
    {
        var lower = name.ToLowerInvariant();
        var sb = new StringBuilder(lower.Length);

        foreach (var ch in lower)
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
            {
                sb.Append(ch);
            }
            else if (ch == '_' || ch == '-')
            {
                sb.Append('-');
            }
        }

        return sb.ToString().Trim('-');
    }
}