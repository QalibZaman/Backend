using System.Text;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.ICategoryService;

public class CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger) : ICategoryService
{
    private static readonly Regex UrlPattern = new("^/[a-z0-9/-]+$", RegexOptions.Compiled);
    private static readonly HashSet<string> ReservedUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        "/admin",
        "/login",
        "/cart",
        "/checkout",
        "/api",
        "/account",
        "/settings",
    };

    public string BuildCategoryUrl(string categoryName, string? requestedUrl, string? parentUrl)
    {
        var normalizedParent = NormalizePath(parentUrl);

        if (!string.IsNullOrWhiteSpace(requestedUrl))
        {
            var normalizedRequested = NormalizePath(requestedUrl);
            if (normalizedRequested != null)
            {
                ValidateNormalizedUrl(normalizedRequested, normalizedParent);
                return normalizedRequested;
            }
        }

        var slug = BuildSlug(categoryName);
        if (slug.Length < 3)
        {
            throw new BadRequestException("Category slug must be at least 3 characters long.");
        }

        var generated = normalizedParent != null
            ? $"{normalizedParent}/{slug}"
            : $"/{slug}";

        ValidateNormalizedUrl(generated, normalizedParent);
        return generated;
    }

    public async Task DeleteCategoryAsync(int id, int shopId, CancellationToken cancellationToken = default)
    private static readonly Regex UrlPattern = new("^/[a-z0-9/-]+$", RegexOptions.Compiled);
    private static readonly HashSet<string> ReservedUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        "/admin",
        "/login",
        "/cart",
        "/checkout",
        "/api",
        "/account",
        "/settings",
    };

    public string BuildCategoryUrl(string categoryName, string? requestedUrl, string? parentUrl)
    {
        var normalizedParent = NormalizePath(parentUrl);

        if (!string.IsNullOrWhiteSpace(requestedUrl))
        {
            var normalizedRequested = NormalizePath(requestedUrl);
            ValidateNormalizedUrl(normalizedRequested, normalizedParent);
            return normalizedRequested;
        }

        var slug = BuildSlug(categoryName);
        if (slug.Length < 3)
        {
            throw new BadRequestException("Category slug must be at least 3 characters long.");
        }

        var generated = normalizedParent != null
            ? $"{normalizedParent}/{slug}"
            : $"/{slug}";

        ValidateNormalizedUrl(generated, normalizedParent);
        return generated;
    }

    public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var specification = new GetCategoryByIdAndShopIdSpecification(id, shopId);
        specification.AddInclude(c => c.InventoryItems);

        var category = await categoryRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException($"There is no category with Id: {id}");

        var conflictModelSpecification = new GetCategoriesOfParentByIdSpecification(id);
        var subcategoryExists = await categoryRepository.Exists(conflictModelSpecification, cancellationToken);

        if (subcategoryExists)
        {
            logger.LogWarning("Attempt to delete category with Id {CategoryId} that has subcategories.", id);
            throw new BadRequestException($"Cannot delete category with Id {id} as it has subcategories.");
        }

        if (category.InventoryItems.Any(i => i.Status != InventoryItemStatusEnum.Unavailable))
        {
            logger.LogWarning("Attempt to delete category with Id {CategoryId} that contains products.", id);
            throw new ConflictException($"Cannot delete category with Id {id} as it contains products.");
        }

        await categoryRepository.DeleteAsync(specification, cancellationToken);
        logger.LogInformation("Category deleted successfully. Id: {CategoryId}", id);
    }

    private static string? NormalizePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().Replace('\\', '/');

        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        while (normalized.Contains("//", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("//", "/");
        }

        normalized = normalized.TrimEnd('/');

        return normalized.ToLowerInvariant();
    }

    private static string BuildSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var lower = name.ToLowerInvariant();
        var sb = new StringBuilder(lower.Length);
        var lastWasHyphen = false;

        foreach (var ch in lower)
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
            {
                sb.Append(ch);
                lastWasHyphen = false;
                continue;
            }

            if (char.IsWhiteSpace(ch) || ch == '-')
            {
                if (!lastWasHyphen)
                {
                    sb.Append('-');
                    lastWasHyphen = true;
                }
            }
        }

        return sb.ToString().Trim('-');
    }

    private static void ValidateNormalizedUrl(string url, string? parentUrl)
    {
        if (string.IsNullOrWhiteSpace(url) || url == "/")
        {
            throw new BadRequestException("Category URL cannot be empty.");
        }

        if (url.Length > 255)
        {
            throw new BadRequestException("Category URL must be 255 characters or less.");
        }

        if (url.EndsWith("/", StringComparison.Ordinal))
        {
            throw new BadRequestException("Category URL cannot have a trailing slash.");
        }

        if (url.Contains("//", StringComparison.Ordinal))
        {
            throw new BadRequestException("Category URL cannot contain empty segments.");
        }

        if (!UrlPattern.IsMatch(url))
        {
            throw new BadRequestException("Category URL contains invalid characters. Allowed: lowercase letters, digits, hyphens, and slashes.");
        }

        if (ReservedUrls.Contains(url))
        {
            throw new BadRequestException("Category URL is reserved.");
        }

        var lastSegment = url[(url.LastIndexOf('/') + 1)..];
        if (lastSegment.Length < 3)
        {
            throw new BadRequestException("Category slug must be at least 3 characters long.");
        }

        if (!string.IsNullOrWhiteSpace(parentUrl))
        {
            if (!url.StartsWith(parentUrl + "/", StringComparison.Ordinal))
            {
                throw new BadRequestException("Category URL must start with the parent category path.");
            }

            var remainder = url[(parentUrl.Length + 1)..];
            if (remainder.Contains('/', StringComparison.Ordinal))
            {
                throw new BadRequestException("Category URL must contain exactly one segment after the parent path.");
            }
        }
        else if (url.IndexOf('/', 1) >= 0)
        {
            throw new BadRequestException("Root category URL must not contain nested segments.");
        }
    }
}
