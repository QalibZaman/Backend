using FluentValidation;
using NettoRetail.Application.Models.CsvFileModel;
using System.Text.RegularExpressions;

namespace NettoRetail.API.Infrastructure.Validators;

public class InventoryItemCsvValidator : AbstractValidator<InventoryItemCsvModel>
{
    public InventoryItemCsvValidator()
    {
        RuleFor(x => x.ProductUpc)
            .NotEmpty().WithMessage("Product UPC must be exactly 12 digits.")
            .Matches(@"^\d{12}$")
            .WithMessage("Product UPC must be exactly 12 digits.");

        RuleFor(x => x.ProductName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .Length(2, 50)
            .Matches(@"^[A-Za-z0-9\-_ ]+$")
            .WithMessage("Product name must only contain letters, digits, spaces, '-' and '_'.");

        RuleFor(x => x.ProductCategory)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Product category is required.")
            .Must(category => !category.StartsWith(" ") && !category.EndsWith(" "))
            .WithMessage("Product category must not have leading or trailing spaces.");

        RuleFor(x => x.ProductLocation)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Length(2, 100)
            .Matches(@"^[A-Za-z0-9\s\-,]+$")
            .WithMessage("Location contains invalid characters.")
            .Must(l => !l.StartsWith(" ") && !l.EndsWith(" "))
            .WithMessage("No leading/trailing spaces.");

        RuleFor(x => x.Brand)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Brand is required.")
            .Length(2, 90)
            .Must(b => !b.StartsWith(" ") && !b.EndsWith(" "))
            .WithMessage("Brand must not have leading or trailing spaces.")
            .Matches(@"^[A-Za-z0-9\s\-_.,]+$")
            .WithMessage("Brand contains invalid characters.");

        RuleFor(x => x.AvailableAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Available quantity must be zero or positive.");

        RuleFor(x => x.ReservedAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Reserved quantity must be zero or positive.")
            .Must((model, reserved) => reserved <= model.OnHandAmount)
            .WithMessage("Reserved quantity cannot exceed on-hand quantity.")
            .Must((model, reserved) => reserved <= model.AvailableAmount)
            .WithMessage("Reserved quantity cannot exceed available quantity.");

        RuleFor(x => x.OnHandAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("On-hand quantity must be zero or positive.")
            .Must((model, onHand) => onHand >= model.AvailableAmount)
            .WithMessage("On-hand quantity must be at least the available quantity.")
            .Must((model, onHand) => onHand >= model.AvailableAmount + model.ReservedAmount)
            .WithMessage("On-hand quantity must accommodate both available and reserved stock.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .ScalePrecision(2, 18)
            .WithMessage("Price must have up to 2 decimal places.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be a valid enum value.");

        RuleFor(x => x.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(500)
            .Matches(@"^[A-Za-z0-9\s.,;:!?\-_'""()]+$")
            .WithMessage("Description contains invalid characters.")
            .Must(desc => !desc.StartsWith(" ") && !desc.EndsWith(" "))
            .Must(desc => !Regex.IsMatch(desc, @"<[^>]+>"))
            .WithMessage("No HTML tags allowed.");
    }
}
