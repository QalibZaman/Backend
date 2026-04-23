using System.Text.RegularExpressions;
using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Domain.Enums;

namespace NettoRetail.API.Infrastructure.Validators;

public class AvailabilityInfoValidator : AbstractValidator<AvailabilityInfoModel>
{
    public AvailabilityInfoValidator()
    {
        RuleFor(model => model.AvailableAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Invalid number. Please check format.");

        RuleFor(model => model.IncomingAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Invalid number. Please check format.");

        RuleFor(model => model.ReservedAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Invalid number. Please check format.")
            .LessThanOrEqualTo(model => model.AvailableAmount)
            .WithMessage("Reserved amount cannot exceed available amount.");

        RuleFor(model => model.OnHandAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Invalid number. Please check format.")
            .Must((model, onHandAmount) => model.AvailableAmount + model.ReservedAmount <= onHandAmount)
            .WithMessage("The sum of Reserved Amount and Available Amount should not exceed the On-hand Amount.");

        RuleFor(model => model.Status)
            .IsInEnum()
            .Must(status => status != InventoryItemStatusEnum.New)
            .WithMessage("Status must be one of: Available, Committed, Unavailable, Incoming.");

        RuleFor(model => model.ProductLocation)
            .NotEmpty().WithMessage("Product location cannot be empty.")
            .Length(2, 100).WithMessage("Location length must be between 2 and 100 characters.")
            .Matches(@"^[a-zA-Z0-9 ,-]+$")
            .WithMessage("Location contains unsupported characters. Only letters, digits, spaces, hyphens, and commas are allowed.")
            .Must(NoLeadingOrTrailingSpaces)
            .WithMessage("Location cannot start or end with a space.")
            .Must(NotContainHtml).WithMessage("Location cannot contain HTML tags.");
    }

    private static bool NoLeadingOrTrailingSpaces(string location)
    {
        if (string.IsNullOrEmpty(location))
        {
            return true;
        }

        return location == location.Trim();
    }

    private static bool NotContainHtml(string location)
    {
        if (string.IsNullOrEmpty(location))
        {
            return true;
        }

        return !Regex.IsMatch(location, @"<[^>]*>");
    }
}
