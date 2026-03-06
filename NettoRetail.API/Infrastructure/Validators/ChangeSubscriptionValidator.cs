using FluentValidation;
using NettoRetail.Application.Models.Request.Subscription;

namespace NettoRetail.API.Infrastructure.Validators;

public class ChangeSubscriptionValidator : AbstractValidator<ChangeSubscriptionRequest>
{
    public ChangeSubscriptionValidator()
    {
        RuleFor(x => x.NewPlan)
            .NotNull().WithMessage("The Plan should be attached");
    }
}