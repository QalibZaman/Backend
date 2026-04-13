using FluentValidation;
using NettoRetail.Application.Models.Request.Category;

namespace NettoRetail.API.Infrastructure.Validators;

public abstract class CategoryUpsertRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : ICategoryUpsertRequest
{
    protected CategoryUpsertRequestValidator()
    {
        RuleFor(model => model.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Category name is required (EM-16).")
            .MinimumLength(2).WithMessage("Category name length must be greater than 2 characters long.")
            .MaximumLength(50).WithMessage("Category name length must be less than 50 characters long.")
            .Matches(@"^[a-zA-Z0-9-_ ]+$").WithMessage("The name can only contain letters, numbers, spaces, hyphens, and underscores. Please check your input.");

        RuleFor(model => model.URL)
            .Cascade(CascadeMode.Stop)
            .Must(BeValidCategoryUrl)
            .WithMessage("Invalid URL format. Use lowercase letters, digits, hyphens, and optional slashes, no trailing slash.")
            .When(model => !string.IsNullOrWhiteSpace(model.URL));

        RuleFor(model => model.ParentId)
            .GreaterThan(0).WithMessage("The id of parent category must be greater than 0")
            .When(model => model.ParentId.HasValue);
    }

    private static bool BeValidCategoryUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true;
        }

        var normalized = url.Trim().Replace('\\', '/');

        if (normalized.EndsWith("/"))
        {
            return false;
        }

        if (normalized.Contains("//"))
        {
            return false;
        }

        if (normalized.Length > 255)
        {
            return false;
        }

        if (normalized.StartsWith('/'))
        {
            normalized = normalized.Substring(1);
        }

        if (normalized.Length == 0)
        {
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, "^[a-z0-9/-]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            return false;
        }

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return false;
        }

        foreach (var segment in segments)
        {
            if (segment.Length < 3)
            {
                return false;
            }
        }

        return true;
    }
}
