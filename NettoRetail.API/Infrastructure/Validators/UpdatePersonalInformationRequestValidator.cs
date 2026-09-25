using FluentValidation;
using NettoRetail.Application.Models.Request;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using System.Text.RegularExpressions;

namespace NettoRetail.Application.Validators.User;

public class UpdatePersonalInformationRequestValidator
    : AbstractValidator<UpdatePersonalInformationRequest>
{
    private const string NamePattern = @"^[A-Za-z]+(?: [A-Za-z]+)*$";

    private const string EmailPattern =
        @"^[A-Za-z0-9_.+\-]+@[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)+$";

    private const string CityPattern =
        @"^[A-Za-z]+(?:[A-Za-z .,'-]*[A-Za-z])?$";

    private readonly ICountryRepository _countryRepository;

    public UpdatePersonalInformationRequestValidator(
        ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;

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
            .MaximumLength(90)
            .Must(NotHaveLeadingOrTrailingSpaces)
            .When(x => !string.IsNullOrWhiteSpace(x.Street))
            .WithMessage("Invalid street.");

        RuleFor(x => x.HouseNumber)
            .MaximumLength(90)
            .Must(NotHaveLeadingOrTrailingSpaces)
            .When(x => !string.IsNullOrWhiteSpace(x.HouseNumber))
            .WithMessage("Invalid house number.");

        RuleFor(x => x.Apartment)
            .MaximumLength(90)
            .Must(NotHaveLeadingOrTrailingSpaces)
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
    }

    private void ConfigureZipCodeRules()
    {
        RuleFor(x => x.ZipCode)
            .Length(3, 8)
            .When(x => !string.IsNullOrWhiteSpace(x.ZipCode))
            .WithMessage("Zip code must contain between 3 and 8 symbols.");

        RuleFor(x => x)
            .CustomAsync(ValidateCountryDependentFieldsAsync);
    }

    private async Task ValidateCountryDependentFieldsAsync(
        UpdatePersonalInformationRequest request,
        ValidationContext<UpdatePersonalInformationRequest> context,
        CancellationToken cancellationToken)
    {
        var hasPhoneNumber = !string.IsNullOrWhiteSpace(request.PhoneNumber);
        var hasZipCode = !string.IsNullOrWhiteSpace(request.ZipCode);

        if (!hasPhoneNumber && !hasZipCode)
            return;

        if (!request.CountryId.HasValue)
        {
            if (hasPhoneNumber)
            {
                context.AddFailure(
                    nameof(request.PhoneNumber),
                    "Country is required when a phone number is provided.");
            }

            if (hasZipCode)
            {
                context.AddFailure(
                    nameof(request.ZipCode),
                    "Country is required when a zip code is provided.");
            }

            return;
        }

        var country = await _countryRepository.GetAsync(
            new GetCountryByIdSpecification(request.CountryId.Value),
            cancellationToken);

        if (country is null)
        {
            context.AddFailure(
                nameof(request.CountryId),
                "Selected country does not exist.");

            return;
        }

        if (hasPhoneNumber &&
            !IsValidPhoneNumber(
                request.PhoneNumber!,
                country.PhoneCode,
                country.PhoneMask))
        {
            context.AddFailure(
                nameof(request.PhoneNumber),
                "Phone number does not match the selected country's format.");
        }

        if (hasZipCode &&
            !IsValidZipCode(
                request.ZipCode!,
                country.ZipCodeMask))
        {
            context.AddFailure(
                nameof(request.ZipCode),
                "Zip code does not match the selected country's format.");
        }
    }

    private static bool IsValidPhoneNumber(
        string phoneNumber,
        string phoneCode,
        string phoneMask)
    {
        if (!phoneNumber.StartsWith(
                phoneCode,
                StringComparison.Ordinal))
        {
            return false;
        }

        var numberWithoutCountryCode = phoneNumber[phoneCode.Length..];

        if (!numberWithoutCountryCode.StartsWith(' '))
            return false;

        var phoneNumberWithoutCode =
            numberWithoutCountryCode.TrimStart();

        if (!Regex.IsMatch(
                phoneNumberWithoutCode,
                phoneMask))
        {
            return false;
        }

        var digitCount = phoneNumber.Count(char.IsDigit);

        return digitCount is >= 8 and <= 15;
    }

    private static bool IsValidZipCode(
        string zipCode,
        string zipCodeMask)
    {
        if (zipCode.Length is < 3 or > 8)
            return false;

        return Regex.IsMatch(zipCode, zipCodeMask);
    }

    private static bool HaveValidPhoneBasicFormat(
        string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true;

        return Regex.IsMatch(
            phoneNumber,
            @"^\+[0-9]+(?: [0-9]+)*$");
    }

    private static bool HaveValidPhoneDigitCount(
        string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true;

        var digitCount = phoneNumber.Count(char.IsDigit);

        return digitCount is >= 8 and <= 15;
    }

    private static bool NotHaveLeadingOrTrailingSpaces(
        string? value)
    {
        return value is null ||
               value == value.Trim();
    }
}
