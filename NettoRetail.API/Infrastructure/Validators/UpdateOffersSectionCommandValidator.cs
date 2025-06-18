using FluentValidation;
using NettoRetail.Application.CQRS.OfferSections.Command;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateOffersSectionCommandValidator : AbstractValidator<UpdateOffersSectionCommand>
{
    public UpdateOffersSectionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Section name is required.")
            .MaximumLength(150); 

        RuleFor(x => x.InventoryItemIds)
            .NotNull().WithMessage("Products list cannot be null.")
            .Must(list => list.Any()).WithMessage("At least one product must be added.");
    }
}