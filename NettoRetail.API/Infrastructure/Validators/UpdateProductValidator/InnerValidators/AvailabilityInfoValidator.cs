using System.Text.RegularExpressions;
using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.API.Infrastructure.Validators;

public class AvailabilityInfoValidator : AbstractValidator<AvailabilityInfoModel>
{
    public AvailabilityInfoValidator()
    {
        RuleFor(model => model.AvailableAmount)
            .NotNull()
            .WithMessage("Availability amount is required.")
            .GreaterThan(0)
            .WithMessage("Availability amount should be greater than zero.");
        RuleFor(model => model.IncomingAmount)
            .NotNull()
            .WithMessage("Incoming amount is required.")
            .GreaterThan(0)
            .WithMessage("Incoming amount should be greater than zero.");
        RuleFor(model => model.ReservedAmount)
            .NotNull()
            .WithMessage("Reserved amount is required.")
            .GreaterThan(0)
            .WithMessage("Reserved amount should be greater than zero.");
        RuleFor(model => model.OnHandAmount)
            .NotNull()
            .WithMessage("On hand amount is required.")
            .GreaterThan(0)
            .WithMessage("On hand amount should be greater than zero.");
        RuleFor(model => model.ProductLocation)
            .NotEmpty().WithMessage("Product location cannot be empty.")
            .Length(2, 100).WithMessage("Location length must be between 2 and 100 characters.")
            .Matches(@"^[a-zA-Z0-9 ,-]+$")
            .WithMessage(
                "Location contains unsupported characters. Only letters, digits, spaces, hyphens, and commas are allowed.")
            .Must(NoLeadingOrTrailingSpaces)
            .WithMessage("Location cannot start or end with a space.")
            .Must(NotContainHtml).WithMessage("Location cannot contain HTML tags.");
    }

    private bool NoLeadingOrTrailingSpaces(string location)
    {
        if (string.IsNullOrEmpty(location)) return true;
        return location == location.Trim();
    }

    private bool NotContainHtml(string location)
    {
        if (string.IsNullOrEmpty(location)) return true;
        return !Regex.IsMatch(location, @"<[^>]*>");
    }
}