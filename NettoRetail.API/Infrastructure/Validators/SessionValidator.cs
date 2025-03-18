using System.Net;
using FluentValidation;
using NettoRetail.Application.Models.Entities;

namespace NettoRetail.API.Infrastructure.Validators;

public class SessionValidator : AbstractValidator<SessionModel>
{
    public SessionValidator()
    {
        RuleFor(x => x.Device)
            .NotEmpty().WithMessage("Device is required.")
            .Matches(@"^[A-Za-z0-9.,\- ]+$").WithMessage("Device contains invalid characters.")
            .MaximumLength(50).WithMessage("Device must not exceed 50 characters.")
            .Must(x => x.Trim() == x).WithMessage("Device must not have leading or trailing spaces.");

        RuleFor(x => x.Browser)
            .NotEmpty().WithMessage("Browser is required.")
            .Matches(@"^[A-Za-z0-9.,\- ]+$").WithMessage("Browser contains invalid characters.")
            .MaximumLength(50).WithMessage("Browser must not exceed 50 characters.")
            .Must(x => x.Trim() == x).WithMessage("Browser must not have leading or trailing spaces.");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("IP Address is required.")
            .MaximumLength(45).WithMessage("IP Address must not exceed 45 characters.")
            .Must(BeAValidIpAddress).WithMessage("Invalid IP Address format.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .Matches("^[A-Za-z ]+, [A-Za-z ]+$").WithMessage("Location must be in 'City, Country' format and contain only letters and spaces.")
            .MaximumLength(100).WithMessage("Location must not exceed 100 characters.")
            .Must(x => x.Trim() == x).WithMessage("Location must not have leading or trailing spaces.");
    }

    private bool BeAValidIpAddress(string ip)
    {
        return IPAddress.TryParse(ip, out _);
    }
}
