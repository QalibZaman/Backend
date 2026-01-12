using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.Order;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetFilteredOrdersValidator : AbstractValidator<GetFilteredOrdersModel>
{
    public GetFilteredOrdersValidator()
    {
        RuleForEach(x => x.CustomerFirstNames)
            .MaximumLength(30)
            .Matches(@"^[\p{L}\p{P}\p{N}\s-]*$")
            .WithMessage("Invalid first name characters at index.");

        RuleForEach(x => x.CustomerLastNames)
            .MaximumLength(30)
            .Matches(@"^[\p{L}\p{P}\p{N}\s-]*$")
            .WithMessage("Invalid last name characters at index.");

        RuleForEach(x => x.CustomerEmails)
            .MaximumLength(256)
            .EmailAddress()
            .WithMessage("Invalid email format at index.");

        RuleForEach(x => x.Creators).ChildRules(x =>
        {
            x.RuleFor(v => v)
                .MaximumLength(50)
                .Matches(@"^[\p{L}\p{P}\p{N}\s-]*$").WithMessage("Invalid characters in creator.");
        });

        RuleForEach(x => x.OrderStatuses).ChildRules(x =>
        {
            x.RuleFor(v => v)
                .MustBeAValidOrderStatus().WithMessage("Invalid order status.");
        });

        RuleForEach(x => x.DeliveryTypes).ChildRules(x =>
        {
            x.RuleFor(v => v)
                .MustBeAValidOrderDeliveryType().WithMessage("Invalid delivery type.");
        });

        RuleForEach(x => x.OrderIds).ChildRules(id =>
        {
            id.RuleFor(v => v)
                .GreaterThan(0).WithMessage("Order ID must be greater than zero.");
        });

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Start date must be before or equal to end date.");

        RuleForEach(x => x.SortingOptions).ChildRules(s =>
        {
            s.RuleFor(x => x.Field)
                .NotEmpty().WithMessage("Sorting field required.");
        });
    }
}