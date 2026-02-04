using FluentValidation;
using NettoRetail.Application.Models.Request.Staff;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class GetFilteredStaffRequestValidator : AbstractValidator<GetFilteredStaffRequest>
    {
        public GetFilteredStaffRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .Must(BeValidName)
                .When(x => !string.IsNullOrWhiteSpace(x.FirstName))
                .WithMessage("Invalid First/Last Name. Please check format or length.");

            RuleFor(x => x.LastName)
                .Must(BeValidName)
                .When(x => !string.IsNullOrWhiteSpace(x.LastName))
                .WithMessage("Invalid First/Last Name. Please check format or length.");
        }

        private static bool BeValidName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (value != value.Trim()) return false;
            if (value.Length < 2) return false;
            return value.All(c => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z');
        }
    }
}
