using FluentValidation;
using NettoRetail.Application.Models.Offer;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateOfferSectionRequestValidator : AbstractValidator<UpdateOfferSectionRequest>
{
    public UpdateOfferSectionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Section name is required.")
            .MaximumLength(60).WithMessage("Name length must be less or equal to 60 symbols")
            .MinimumLength(2).WithMessage("Name length must be more or equal to 2 symbols")
            .Matches(@"^[A-Za-z0-9._,\- ]+$").WithMessage("Name contains invalid characters")
            .Matches(@"^\S(.*\S)?$").WithMessage("Name must not start or end with spaces.");

        RuleFor(x => x.InventoryItemIds)
                .NotNull().WithMessage("Products list cannot be null.")
                .Must(list => list.Any()).WithMessage("At least one product must be added.");

        RuleFor(x => x.SelectionType)
            .IsInEnum().WithMessage("Invalid selection type");
    }
}
