using System.Text.RegularExpressions;
using FluentValidation;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetInventoryItemsByUpcOrNameValidator : AbstractValidator<GetInventoryItemsByUpcOrNameModel>
{
    public GetInventoryItemsByUpcOrNameValidator()
    {
        RuleFor(x => x.UpcOrName)
            .NotEmpty().NotNull().WithMessage("UPC or Product Name is required.")
            .Must(BeAValidUpcOrProductName)
            .WithMessage("Input must be a valid UPC (12-digit numeric) or a valid product name (up to 50 characters, only letters, digits, '-' and '_').");
    }

    private bool BeAValidUpcOrProductName(string input)
    {
        string productNamePattern = "^[a-zA-Z0-9-_\\s]{1,50}$";

        return Regex.IsMatch(input, productNamePattern);
    }
}
