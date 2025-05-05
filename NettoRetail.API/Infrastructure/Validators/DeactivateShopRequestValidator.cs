using FluentValidation;
using NettoRetail.Application.Models.Shop;

namespace NettoRetail.API.Infrastructure.Validators;

public class DeactivateShopRequestValidator : AbstractValidator<DeactivateShopRequest>
{
    public DeactivateShopRequestValidator()
    {
        RuleFor(x => x.Reason)
           .NotEmpty().WithMessage("Deactivation reason is required.")
           .NotNull().WithMessage("Deactivation reason is required.")
           .MaximumLength(100).WithMessage("Please check format or length.");

        RuleFor(x => x.DeactivationDate)
            .GreaterThan(DateTime.UtcNow.Date).WithMessage("Deactivation date must be in the future.");
    }
}
