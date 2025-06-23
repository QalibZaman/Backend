using FluentValidation;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UpdateSubscriptionPlanValidator : AbstractValidator<UpdateSubscriptionPlanRequest>
    {
        public UpdateSubscriptionPlanValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("\"Field name\" cannot be empty.")
                .NotNull().WithMessage("\"Field name\" cannot be empty.")
                .MinimumLength(2).WithMessage("\"Field name\" length must be greater than 2 characters long.")
                .MaximumLength(25).WithMessage("\"Field name\" length must be less than 50 characters long.")
                .Matches(@"^[A-Za-z0-9_-](?:[A-Za-z0-9 _-]*[A-Za-z0-9_-])?$").WithMessage("The name can only contain letters, numbers, hyphens, underscores and not start or end with a space. Please check your input.")
                .Must(name => !System.Text.RegularExpressions.Regex.IsMatch(name ?? "", @"(--|__|-_|_-)")).WithMessage("Name must not contain consecutive special characters: --, __, -_, or _-");

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("\"Field price\" cannot be empty.")
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("\"Field description\" cannot be empty.")
                .NotNull().WithMessage("\"Field description\" cannot be empty.")
                .MaximumLength(500).WithMessage("\"Field description\" length must be less than 50 characters long.")
                .Matches(@"^[A-Za-z0-9_-](?:[A-Za-z0-9 _-]*[A-Za-z0-9_-])?$").WithMessage("The description can only contain letters, numbers, hyphens, underscores and not start or end with a space. Please check your input.");

            RuleFor(x => x.Motto)
                .NotEmpty().WithMessage("\"Field motto\" cannot be empty.")
                .NotNull().WithMessage("\"Field motto\" cannot be empty.")
                .MaximumLength(70).WithMessage("\"Field motto\" length must be less than 50 characters long.")
                .Matches(@"^[A-Za-z0-9_-](?:[A-Za-z0-9 _-]*[A-Za-z0-9_-])?$").WithMessage("The motto can only contain letters, numbers, hyphens, underscores and not start or end with a space. Please check your input.");

            RuleFor(x => x.FeatureIds)
                .NotNull().WithMessage("At least one feature must be enabled")
                .Must(f => f != null && f.Count > 0).WithMessage("At least one feature must be enabled.");
        }
    }
}
