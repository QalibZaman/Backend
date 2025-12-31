using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.Order;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetFilteredOrdersValidator : AbstractValidator<GetFilteredOrdersModel>
{
    private const string LatinTextRegex = @"^[a-zA-Z0-9\s-]*$";

    public GetFilteredOrdersValidator()
    {
        RuleForEach(x => x.CustomerFirstNames).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MaximumLength(30)
                .Matches(LatinTextRegex)
                .When(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage("Invalid first name characters.");
        });

        RuleForEach(x => x.CustomerLastNames).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MaximumLength(30)
                .Matches(LatinTextRegex)
                .When(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage("Invalid last name characters.");
        });

        RuleForEach(x => x.CustomerEmails).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MaximumLength(256)
                .When(v => !string.IsNullOrWhiteSpace(v));

            r.RuleFor(v => v)
                .EmailAddress()
                .When(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage("Invalid email format.");
        });

        RuleForEach(x => x.Creators).ChildRules(r =>
        {
            r.RuleFor(v => v)
                .MaximumLength(50)
                .Matches(LatinTextRegex)
                .When(v => !string.IsNullOrWhiteSpace(v))
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
