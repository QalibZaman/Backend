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
            .Matches(@"^[a-zA-Z0-9-_]+$").WithMessage("The name can only contain letters, numbers, hyphens, and underscores. Please check your input.");

        RuleFor(model => model.URL)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("URL is required (EM-16).")
            .Length(1, 50).WithMessage("The URL must be between 1 and 50 characters long.")
            .Matches(@"^[a-zA-Z0-9\-._~]+$").WithMessage("Allowed characters: All letters (A-Z, a-z), digits (0-9), -, ., _, ~.")
            .Matches(@"^\S.*\S$").WithMessage("No leading or trailing spaces are allowed.");

        RuleFor(model => model.ParentId)
            .GreaterThan(0).WithMessage("The id of parent category must be greater than 0")
            .When(model => model.ParentId.HasValue);
    }
}
