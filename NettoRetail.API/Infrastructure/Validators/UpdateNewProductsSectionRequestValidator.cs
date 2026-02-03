using System.Text.RegularExpressions;
using FluentValidation;
using NettoRetail.Application.Models.NewProductsSection;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateNewProductsSectionRequestValidator : AbstractValidator<UpdateNewProductsSectionRequest>
{
    public UpdateNewProductsSectionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("Section name is required.")
            .NotEmpty().WithMessage("Section name is required.")
            .Length(2, 60).WithMessage("Name must be between 2 and 60 characters long.")
            .Must(name => name == name.Trim()).WithMessage("Name must not start or end with a space.")
            .Must(name => name.All(c => c <= 127)).WithMessage("Only Latin (ASCII) characters are allowed.")
            .Matches(new Regex(@"^[A-Za-z0-9 _\-\.,()]+$", RegexOptions.CultureInvariant))
            .WithMessage("Name contains invalid characters.");

        RuleFor(x => x.InventoryItemIds)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Products list cannot be null.")
            .Must(list => list.Any()).WithMessage("At least one product must be added.")
            .Must(list => list.Count == list.Distinct().Count()).WithMessage("Products list must contain only unique values.");

        RuleFor(x => x.SelectionType)
            .IsInEnum().WithMessage("Invalid products selection type value.");
    }
}