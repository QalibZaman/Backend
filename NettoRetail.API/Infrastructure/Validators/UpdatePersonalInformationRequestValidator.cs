using FluentValidation;
using NettoRetail.Application.Models.Request;
using NettoRetail.Shared.Constants;
using System.Text.RegularExpressions;

namespace NettoRetail.Application.Validators.User;

public class UpdatePersonalInformationRequestValidator
    : AbstractValidator<UpdatePersonalInformationRequest>
{
    private const string NamePattern = @"^[A-Za-z]+(?: [A-Za-z]+)*$";

    private const string EmailPattern =
        @"^[A-Za-z0-9](?:[A-Za-z0-9_.+\-]*[A-Za-z0-9])?@[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?(?:\.[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?)+$";

    private const string CityPattern =
        @"^[A-Za-z]+(?:[A-Za-z .,'-]*[A-Za-z])?$";

    private const string AddressPattern =
        @"^[A-Za-z0-9](?:[A-Za-z0-9 .,'/#()\-]*[A-Za-z0-9])?$";

    public UpdatePersonalInformationRequestValidator()
    {
        ConfigureNameRules();
        ConfigureEmailRules();
        ConfigurePhoneRules();
        ConfigureAddressRules();
        ConfigureCityRules();
        ConfigureStateRules();
        ConfigureCountryRules();
        ConfigureZipCodeRules();
    }

    private void ConfigureNameRules()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(2, 30)
            .Matches(NamePattern)
            .WithMessage(
                ValidationErrorMessages.Staff.FirstNameIsInvalidErrorMessage);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .Length(2, 30)
            .Matches(NamePattern)
            .WithMessage(
                ValidationErrorMessages.Staff.LastNameIsInvalidErrorMessage);
    }

    private void ConfigureEmailRules()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(256)
            .Matches(EmailPattern)
            .WithMessage(
                ValidationErrorMessages.Staff.EmailIsInvalidErrorMessage);
    }

    private void ConfigurePhoneRules()
    {
        RuleFor(x => x.PhoneNumber)
            .Must(HaveValidPhoneBasicFormat)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Invalid phone number format.");

        RuleFor(x => x.PhoneNumber)
            .Must(HaveValidPhoneDigitCount)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage(
                "Phone number must contain between 8 and 15 digits, excluding '+'.");
    }

    private void ConfigureAddressRules()
    {
        RuleFor(x => x.Street)
            .Length(1, 90)
            .Matches(AddressPattern)
            .When(x => !string.IsNullOrWhiteSpace(x.Street))
            .WithMessage("Invalid street.");

        RuleFor(x => x.HouseNumber)
            .Length(1, 90)
            .Matches(AddressPattern)
            .When(x => !string.IsNullOrWhiteSpace(x.HouseNumber))
            .WithMessage("Invalid house number.");

        RuleFor(x => x.Apartment)
            .Length(1, 90)
            .Matches(AddressPattern)
            .When(x => !string.IsNullOrWhiteSpace(x.Apartment))
            .WithMessage("Invalid apartment.");
    }

    private void ConfigureCityRules()
    {
        RuleFor(x => x.City)
            .Length(3, 90)
            .Matches(CityPattern)
            .When(x => !string.IsNullOrWhiteSpace(x.City))
            .WithMessage("Invalid city.");
    }

    private void ConfigureStateRules()
    {
        RuleFor(x => x.State)
            .Length(5, 90)
            .Must(NotHaveLeadingOrTrailingSpaces)
            .When(x => !string.IsNullOrWhiteSpace(x.State))
            .WithMessage("Invalid state/province.");
    }

    private void ConfigureCountryRules()
    {
        RuleFor(x => x.CountryId)
            .GreaterThan(0)
            .When(x => x.CountryId.HasValue)
            .WithMessage("Invalid country.");

        RuleFor(x => x.CountryId)
            .NotNull()
            .When(x =>
                !string.IsNullOrWhiteSpace(x.PhoneNumber) ||
                !string.IsNullOrWhiteSpace(x.ZipCode))
            .WithMessage(
                "Country is required when phone number or zip code is provided.");
    }

    private void ConfigureZipCodeRules()
    {
        RuleFor(x => x.ZipCode)
            .Length(3, 8)
            .When(x => !string.IsNullOrWhiteSpace(x.ZipCode))
            .WithMessage(
                "Zip code must contain between 3 and 8 symbols.");
    }

    private static bool HaveValidPhoneBasicFormat(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true;

        return Regex.IsMatch(
            phoneNumber,
            @"^\+[0-9]+(?: [0-9]+)*$");
    }

    private static bool HaveValidPhoneDigitCount(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true;

        var digitCount = phoneNumber.Count(char.IsDigit);

        return digitCount is >= 8 and <= 15;
    }

    private static bool NotHaveLeadingOrTrailingSpaces(string? value)
    {
        return value is null || value == value.Trim();
    }
}