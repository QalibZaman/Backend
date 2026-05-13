using System.Text.RegularExpressions;
using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public partial class GetFilteredOrdersValidator : AbstractValidator<GetFilteredOrdersModel>
{
    private static readonly Regex LatinTextRegex = LatinTextRegexGenerated();

    [GeneratedRegex("^[a-zA-Z0-9\\s\\-._',]*$", RegexOptions.CultureInvariant)]
    private static partial Regex LatinTextRegexGenerated();

    public GetFilteredOrdersValidator()
    {
        RuleForEach(x => x.CustomerFirstNames).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MinimumLength(2)
                .MaximumLength(30)
                .Must(v => string.IsNullOrWhiteSpace(v) || LatinTextRegex.IsMatch(v))
                .WithMessage(ValidationErrorMessages.Name.PatternErrorMessage);
        });

        RuleForEach(x => x.CustomerLastNames).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MinimumLength(2)
                .MaximumLength(30)
                .Must(v => string.IsNullOrWhiteSpace(v) || LatinTextRegex.IsMatch(v))
                .WithMessage(ValidationErrorMessages.Name.PatternErrorMessage);
        });

        RuleForEach(x => x.CustomerEmails).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MinimumLength(2)
                .MaximumLength(256)
                .When(v => !string.IsNullOrWhiteSpace(v));

            r.RuleFor(v => v)
                .EmailAddress()
                .When(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(ValidationErrorMessages.Common.InvalidEmailFormatErrorMessage);
        });

        RuleForEach(x => x.Creators).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MaximumLength(61)
                .Must(v => string.IsNullOrWhiteSpace(v) || LatinTextRegex.IsMatch(v))
                .WithMessage(ValidationErrorMessages.Name.PatternErrorMessage);
        });

        RuleForEach(x => x.OrderStatuses).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MustBeAValidOrderStatus()
                .When(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(ValidationErrorMessages.Order.InvalidOrderStatusErrorMessage);
        });

        RuleForEach(x => x.DeliveryTypes).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MustBeAValidOrderDeliveryType()
                .When(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(ValidationErrorMessages.Order.InvalidDeliveryErrorMessage);
        });

        RuleForEach(x => x.OrderIds).ChildRules(r =>
        {
            r.RuleFor(x => x)
                .GreaterThan(0);
        });

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage(ValidationErrorMessages.Common.InvalidDateRangeErrorMessage);

        RuleForEach(x => x.SortingOptions).ChildRules(s =>
        {
            s.RuleFor(x => x.Field)
                .NotEmpty()
                .WithMessage(ValidationErrorMessages.Sort.SortingFieldRequiredMessage);
        });
    }
}
