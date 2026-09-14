using FluentValidation;
using NettoRetail.Application.Models.Request.Category;

namespace NettoRetail.API.Infrastructure.Validators;

public class AddCategoryRequestValidator : AbstractValidator<AddCategoryRequest>
{
    private const string NamePattern = "^[a-zA-Z0-9_-]{2,50}$";

    public AddCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("This field cannot be empty") // EM-3
            .Matches(NamePattern)
            .WithMessage("Invalid category name"); // EM-34

        RuleFor(x => x.ParentId)
            .GreaterThan(0)
            .WithMessage("The id of parent category must be greater than 0")
            .When(x => x.ParentId.HasValue);
    }
}