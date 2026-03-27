using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Models.Shop;

namespace NettoRetail.API.Infrastructure.Validators;

public class ShopModelValidator : AbstractValidator<ShopModel>
{
    private static readonly string[] ReservedUrls = { "/admin", "/login", "/cart", "/checkout", "/api", "/account", "/settings" };

    public ShopModelValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Length(7, 13).WithMessage("Incorrect phone number length");

        RuleFor(x => x.ZipCode)
            .Length(3, 8).WithMessage("Incorrect zipcode length")
            .When(x => !x.ZipCode.IsNullOrEmpty());

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(2, 90).WithMessage("Name must be between 2 and 90 characters.")
            .Must(x => x.Trim() == x).WithMessage("No leading or trailing spaces are allowed.")
            .Matches(@"^[0-9A-Za-z\-'., &\/#\(\)]+$").WithMessage("Name can only contain Latin characters, digits, and common punctuation ('.', ',', '-', '/', '#', '(', ')', '&', ' ').");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email length must be less than 256 characters.")
            .Matches(@"^(?!.*[_.]{2})[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]+\.[a-zA-Z]{2,}$")
            .WithMessage("Email must be in a valid format.");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("\"URL\" cannot be empty.")
            .MaximumLength(255).WithMessage("URL must not exceed 255 characters.")
            .Must(x => x.Trim() == x).WithMessage("No leading or trailing spaces are allowed.")
            .Must(x => IsValidHierarchicalUrl(x)).WithMessage("URL must correspond to the hierarchical structure: /{parent-slug}/{category-slug}, where each slug portion must be at least 3 characters.")
            .Must(x => !IsReservedUrl(x)).WithMessage("URL cannot use reserved paths: /admin, /login, /cart, /checkout, /api, /account, /settings.");

        RuleFor(x => x.Address)
            .Length(5, 90).WithMessage("Address must be between 5 and 90 characters.")
            .Matches(@"^[A-Za-z0-9\s\-,.;:&'""@#+*]+$").WithMessage("Address can only contain Latin characters, numbers, and special characters.")
            .Must(x => x.Trim() == x).WithMessage("No leading or trailing spaces are allowed.")
            .When(x => !x.Address.IsNullOrEmpty());

        RuleFor(x => x.City)
            .Length(2, 50).WithMessage("City must be between 2 and 50 characters.")
            .Matches(@"^[A-Za-z\s-]+$").WithMessage("City can only contain Latin characters, spaces, and hyphens.")
            .Must(x => x.Trim() == x).WithMessage("No leading or trailing spaces are allowed.")
            .When(x => !x.City.IsNullOrEmpty());

        RuleFor(x => x.State)
            .Length(2, 50).WithMessage("State must be between 2 and 50 characters.")
            .Matches(@"^[A-Za-z0-9\s\-,.;:&'""@#+*]+$").WithMessage("State can only contain Latin characters, numbers, and special characters.")
            .Must(x => x.Trim() == x).WithMessage("No leading or trailing spaces are allowed.")
            .When(x => !x.State.IsNullOrEmpty());

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Invalid description. Please check format or length.")
            .Matches(@"^[A-Za-z0-9\s\-,._!?()]+$").WithMessage("Invalid description. Please check format or length.")
            .Must(x => x.Trim() == x).WithMessage("Invalid description. Please check format or length.")
            .Must((description) => !Regex.IsMatch(description, @"<[^>]+>"))
            .WithMessage("Invalid description. Please check format or length.")
            .When(x => !x.Description.IsNullOrEmpty());
    }

    /// <summary>
    /// Validates that the URL follows the hierarchical structure: /{parent-slug}/{category-slug}
    /// Each slug portion must be at least 3 characters long.
    /// </summary>
    private static bool IsValidHierarchicalUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // URL must start with /
        if (!url.StartsWith("/"))
            return false;

        // Split by / and filter empty parts
        var parts = url.Split(new[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);

        // Must have exactly 2 parts (parent-slug and category-slug)
        if (parts.Length != 2)
            return false;

        // Each part must be at least 3 characters
        if (parts[0].Length < 3 || parts[1].Length < 3)
            return false;

        // Each part should only contain alphanumeric characters and hyphens
        var slugPattern = @"^[a-z0-9-]+$";
        foreach (var part in parts)
        {
            if (!Regex.IsMatch(part, slugPattern, RegexOptions.IgnoreCase))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the URL is in the reserved URLs list.
    /// </summary>
    private static bool IsReservedUrl(string url)
    {
        return ReservedUrls.Any(reserved => url.Equals(reserved, StringComparison.OrdinalIgnoreCase));
    }
}
