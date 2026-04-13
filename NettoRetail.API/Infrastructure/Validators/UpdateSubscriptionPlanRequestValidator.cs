using FluentValidation;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateSubscriptionPlanRequestValidator : AbstractValidator<UpdateSubscriptionPlanRequest>
{
    public UpdateSubscriptionPlanRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
            .NotNull()
            .MinimumLength(2)
            .MaximumLength(25)
            .Matches(ValidationErrorMessages.Subscription.TrimmedAlphanumericRegex).WithMessage(ValidationErrorMessages.Subscription.NameFormatErrorMessage)
            .Must(name => !System.Text.RegularExpressions.Regex.IsMatch(name ?? "", ValidationErrorMessages.Subscription.ConsecutiveSpecialCharsRegex)).WithMessage(ValidationErrorMessages.Subscription.NameConsecutiveSpecialCharsErrorMessage);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage(ValidationErrorMessages.Subscription.PriceRequiredErrorMessage);

        RuleFor(x => x.Description)
            .NotEmpty()
            .NotNull()
            .MaximumLength(500)
            .Matches(ValidationErrorMessages.Subscription.TrimmedAlphanumericRegex).WithMessage(ValidationErrorMessages.Subscription.DescriptionFormatErrorMessage);

        RuleFor(x => x.Motto)
            .NotEmpty()
            .NotNull()
            .MaximumLength(70)
            .Matches(ValidationErrorMessages.Subscription.TrimmedAlphanumericRegex).WithMessage(ValidationErrorMessages.Subscription.MottoFormatErrorMessage);

        RuleFor(x => x.FeatureIds)
            .NotNull().WithMessage(ValidationErrorMessages.Subscription.FeatureIdsRequiredErrorMessage)
            .Must(f => f != null && f.Count > 0).WithMessage(ValidationErrorMessages.Subscription.FeatureIdsRequiredErrorMessage)
            .Must(x => x!.All(id => id > 0)).WithMessage(ValidationErrorMessages.Subscription.FeatureIdsPositiveErrorMessage)
            .Must(x => x!.Distinct().Count() == x!.Count).WithMessage(ValidationErrorMessages.Subscription.FeatureIdsUniqueErrorMessage);
    }
}
