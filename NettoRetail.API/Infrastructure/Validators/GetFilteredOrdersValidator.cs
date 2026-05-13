using System.Text.RegularExpressions;
using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.Order;

namespace NettoRetail.API.Infrastructure.Validators;

public partial class GetFilteredOrdersValidator : AbstractValidator<GetFilteredOrdersModel>
{
    private static readonly Regex LatinTextRegex = LatinTextRegexGenerated();
    private static readonly Regex EmailFilterRegex = EmailFilterRegexGenerated();

    [GeneratedRegex("^[a-zA-Z0-9\\s\\-._',]*$", RegexOptions.CultureInvariant)]
    private static partial Regex LatinTextRegexGenerated();

    [GeneratedRegex("^[a-zA-Z0-9\\s@._+\\-',]*$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailFilterRegexGenerated();

    public GetFilteredOrdersValidator()
    {
        RuleForEach(x => x.CustomerFirstNames).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MaximumLength(30)
                .Must(v => string.IsNullOrWhiteSpace(v) || LatinTextRegex.IsMatch(v))
                .WithMessage("Invalid first name characters.");
        });

        RuleForEach(x => x.CustomerLastNames).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MaximumLength(30)
                .Must(v => string.IsNullOrWhiteSpace(v) || LatinTextRegex.IsMatch(v))
                .WithMessage("Invalid last name characters.");
        });

        RuleForEach(x => x.CustomerEmails).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MaximumLength(256)
                .When(v => !string.IsNullOrWhiteSpace(v));

            r.RuleFor(v => v)
                .Must(v => string.IsNullOrWhiteSpace(v) || EmailFilterRegex.IsMatch(v))
                .When(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage("Invalid email characters.");
        });

        RuleForEach(x => x.Creators).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MaximumLength(50)
                .Must(v => string.IsNullOrWhiteSpace(v) || LatinTextRegex.IsMatch(v))
                .WithMessage("Invalid characters in creator.");
        });

        RuleForEach(x => x.OrderStatuses).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MustBeAValidOrderStatus()
                .When(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage("Invalid order status.");
        });

        RuleForEach(x => x.DeliveryTypes).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MustBeAValidOrderDeliveryType()
                .When(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage("Invalid delivery type.");
        });

        RuleForEach(x => x.OrderIds)
            .GreaterThan(0)
            .WithMessage("Order ID must be greater than zero.");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Start date must be before or equal to end date.");

        RuleForEach(x => x.SortingOptions).ChildRules(s =>
        {
            s.RuleFor(x => x.Field)
                .NotEmpty()
                .WithMessage("Sorting field required.");
        });
    }
}
