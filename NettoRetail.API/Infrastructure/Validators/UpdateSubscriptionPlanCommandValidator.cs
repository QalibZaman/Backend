using FluentValidation;
using NettoRetail.Application.CQRS.Subscriptions.Commands;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateSubscriptionPlanCommandValidator : AbstractValidator<UpdateSubscriptionPlanCommand>
{
    private readonly IValidator<UpdateSubscriptionPlanRequest> _validator;
    public UpdateSubscriptionPlanCommandValidator(IValidator<UpdateSubscriptionPlanRequest> validator)
    {
        _validator = validator;
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PlanId)
            .GreaterThan(0);

        RuleFor(x => x.SubscriptionPlanRequest)
            .SetValidator(_validator);
    }
}
