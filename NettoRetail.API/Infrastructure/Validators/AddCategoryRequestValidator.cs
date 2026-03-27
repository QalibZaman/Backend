using FluentValidation;
using NettoRetail.Application.Models.Request.Category;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class AddCategoryRequestValidator : AbstractValidator<AddCategoryRequest>
    {
        public AddCategoryRequestValidator()
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
    }
}
