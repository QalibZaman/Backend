using FluentValidation;
using NettoRetail.Application.Models.Request.Order;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateOrderRequestModelValidator : AbstractValidator<UpdateOrderRequest>
{
    public UpdateOrderRequestModelValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0);

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Comment)
            .MaximumLength(150)
            .Matches(@"^[A-Za-z0-9\s.,\-_'!?()]*$")
            .When(x => !string.IsNullOrWhiteSpace(x.Comment))
            .Must(c => c == null || c.Trim().Length == c.Length)
            .WithMessage("Comment cannot have leading or trailing spaces.");
    }
}
