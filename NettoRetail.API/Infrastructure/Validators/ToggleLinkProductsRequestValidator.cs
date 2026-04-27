using FluentValidation;
using NettoRetail.Application.Models.Request.InventoryItem;

namespace NettoRetail.API.Infrastructure.Validators;

public class ToggleLinkProductsRequestValidator : AbstractValidator<ToggleLinkProductsRequest>
{
    public ToggleLinkProductsRequestValidator()
    {
        RuleFor(x => x.ParentId)
            .NotEmpty().WithMessage("Parent product ID is required.");

        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child product ID is required.");

        RuleFor(x => x)
            .Must(x => x.ParentId != x.ChildId)
            .WithMessage("Parent and child product IDs must be different.");
    }
}
