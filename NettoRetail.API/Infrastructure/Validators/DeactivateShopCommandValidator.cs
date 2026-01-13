using FluentValidation;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public sealed class DeactivateShopCommandValidator : AbstractValidator<DeactivateShopCommand>
{
    public DeactivateShopCommandValidator()
    {
        RuleFor(x => x.ShopId)
            .GreaterThan(0);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[A-Za-z0-9\s.,;:!?'""()\-\[\]/]+$")
            .WithMessage("Reason contains invalid characters.");

        RuleFor(x => x.DeactivationDate)
            .Must(d => d is null || d.Value != default)
            .WithMessage("DeactivationDate is invalid.");
    }
}
