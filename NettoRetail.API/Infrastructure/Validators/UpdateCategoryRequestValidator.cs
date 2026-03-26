using System.Text.RegularExpressions;
using FluentValidation;
using NettoRetail.Application.Models.Request.Category;

namespace NettoRetail.API.Infrastructure.Validators
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(model => model.Name)
            .NotEmpty().WithMessage("\"Field name\" cannot be empty.")
            .NotNull().WithMessage("\"Field name\" cannot be empty.")
            .MinimumLength(2).WithMessage("\"Field name\" length must be greater than 2 characters long.")
            .MaximumLength(50).WithMessage("\"Field name\" length must be less than 50 characters long.")
            .Matches(@"^[a-zA-Z0-9-_ ]+$").WithMessage("The name can only contain letters, numbers, spaces, hyphens, and underscores. Please check your input.");

        RuleFor(model => model.Visibility)
            .NotNull().WithMessage("\"Visibility\" cannot be null.");

        RuleFor(model => model.ParentId)
            .GreaterThan(0).WithMessage("The id of parent category must be greater than 0");
    }

    private static bool BeValidCategoryUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true;
        }

        var normalized = url.Trim();

        if (!normalized.StartsWith("/"))
        {
            return false;
        }

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

        if (!Regex.IsMatch(normalized, @"^/[a-z0-9/-]+$"))
        {
            return false;
        }

        var lastSegment = normalized[(normalized.LastIndexOf('/') + 1)..];
        if (lastSegment.Length < 3)
        {
            return false;
        }

        return true;
    }
}
