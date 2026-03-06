using FluentValidation;
using NettoRetail.Application.Models.Request.Staff;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetFilteredStaffCsvRequestValidator : AbstractValidator<GetFilteredStaffCsvRequest>
{
    private const string NameRegex = @"^[A-Za-z]+( [A-Za-z]+)*$";

    // Simplified RFC-5321-compatible email pattern
    private const string EmailRegex =
        @"^[A-Za-z0-9._+\-]{2,}@[A-Za-z0-9\-\.]+\.[A-Za-z\.]{2,}$";

    private static readonly string[] AllowedStatuses =
    {
        "Create Password",
        "Online",
        "Offline",
        "Deactivated"
    };

    public GetFilteredStaffCsvRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        When(x => !string.IsNullOrWhiteSpace(x.FirstName), () =>
        {
            RuleFor(x => x.FirstName!)
                .Matches(NameRegex)
                .Length(2, 30)
                .WithMessage("Invalid First Name. Please check format or length.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.LastName), () =>
        {
            RuleFor(x => x.LastName!)
                .Matches(NameRegex)
                .Length(2, 30)
                .WithMessage("Invalid Last Name. Please check format or length.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email!)
                .MaximumLength(256)
                .Matches(EmailRegex)
                .WithMessage("Invalid Email format.");
        });

        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .WithMessage($"Invalid Staff Member Status. Allowed values: {string.Join(", ", AllowedStatuses)}.");

        When(x => x.RoleId.HasValue, () =>
        {
            RuleFor(x => x.RoleId)
                .GreaterThan(0);
        });
    }

    private bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return true;

        string normalizedInput = status.Replace(" ", "").ToLowerInvariant();

        return AllowedStatuses
            .Any(allowed => allowed.Replace(" ", "").ToLowerInvariant() == normalizedInput);
    }
}
