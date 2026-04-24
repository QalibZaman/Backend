using FluentValidation;
using NettoRetail.Application.Models.Request.Address;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressRequestValidator()
    {
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ZipCode)
            .MinimumLength(3)
            .MaximumLength(8);

        RuleFor(x => x.City)
            .MinimumLength(3)
            .MaximumLength(90)
            .Matches(@"^[A-Z][a-zA-Z\s\-]*$")
            .WithMessage(ValidationErrorMessages.Address.InvalidCityFormatErrorMessage);

        RuleFor(x => x.Street)
            .NotNull()
            .Matches(@"^[A-Z][a-zA-Z0-9\s\-\.,']+$")
            .WithMessage(ValidationErrorMessages.Address.InvalidStreetFormatErrorMessage);

        RuleFor(x => x.BuildingNumber)
            .NotNull()
            .Matches(@"^[a-zA-Z0-9\s\-/]+$").WithMessage(ValidationErrorMessages.Address.InvalidBuildingNumberFormatErrorMessage);

        RuleFor(x => x.Entrance)
            .GreaterThan(0);

        RuleFor(x => x.OfficeNumber)
            .Null()
            .When(x => x.ApartmentNumber.HasValue).WithMessage(ValidationErrorMessages.Address.OfficeNumberAndApartmentNumberConflictErrorMessage)
            .GreaterThan(0);

        RuleFor(x => x.ApartmentNumber)
            .Null()
            .When(x => x.OfficeNumber.HasValue).WithMessage(ValidationErrorMessages.Address.OfficeNumberAndApartmentNumberConflictErrorMessage)
            .GreaterThan(0);

        RuleFor(x => x)
            .Must(HaveValidCombinedLength)
            .WithMessage(ValidationErrorMessages.Address.InvalidCombinedAddressFormatErrorMessage)
            .Must(HaveValidFormat)
            .WithMessage(ValidationErrorMessages.Address.InvalidCombinedAddressFormatErrorMessage);
    }

    private bool HaveValidCombinedLength(CreateAddressRequest request)
    {
        string combined = BuildAddressString(request);

        return combined.Length >= 5 && combined.Length <= 90;
    }

    private bool HaveValidFormat(CreateAddressRequest request)
    {
        string combined = BuildAddressString(request);

        if (combined.StartsWith(" ") || combined.EndsWith(" "))
        {
            return false;
        }

        return combined.All(x => x >= 32 && x <= 126);
    }

    private string BuildAddressString(CreateAddressRequest x)
    {
        return $"{x.Street}{x.BuildingNumber}{x.Floor}{x.Entrance}{x.ApartmentNumber}{x.OfficeNumber}";
    }
}