using FluentValidation;
using NettoRetail.Application.Models.CsvFileModel;
using System.Text.RegularExpressions;
public class InventoryItemCsvValidator : AbstractValidator<InventoryItemCsvModel> 
{
    public InventoryItemCsvValidator() {
        RuleFor(x => x.ProductUpc)
            .GreaterThan(0)
            .WithMessage("UPC must be numeric and positive."); 

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .Length(2, 50)
            .Matches(@"^[A-Za-z0-9\s\-_]+$")
            .WithMessage("Product name contains invalid characters."); 
        
        RuleFor(x => x.ProductLocation)
            .NotEmpty()
            .Length(2, 100)
            .Matches(@"^[A-Za-z0-9\s\-,]+$")
            .WithMessage("Location contains invalid characters.")
            .Must(l => !l.StartsWith(" ") && !l.EndsWith(" "))
            .WithMessage("No leading/trailing spaces."); 

        RuleFor(x => x.Brand)
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
            .ScalePrecision(2, 18); 
        
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be a valid enum value."); 
        
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500)
            .Matches(@"^[A-Za-z0-9\s.,;:!?\-_'""()]+$")
            .WithMessage("Description contains invalid characters.")
            .Must(desc => !desc.StartsWith(" ") && !desc.EndsWith(" "))
            .Must(desc => !Regex.IsMatch(desc, @"<[^>]+>"))
            .WithMessage("No HTML tags allowed.");
    } 
}