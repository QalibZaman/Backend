using FluentValidation;
using NettoRetail.Application.CQRS.Users.Commands.PersonalInformation;
using NettoRetail.Application.Models.Request;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.Shared.Interfaces.Repositories;
using System.Text.RegularExpressions;

public class UpdatePersonalInformationCommandValidator
    : AbstractValidator<UpdatePersonalInformationCommand>
{
    private readonly ICountryRepository _countryRepository;

    public UpdatePersonalInformationCommandValidator(
        IValidator<UpdatePersonalInformationRequest> validator,
        ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;

        RuleFor(x => x.Model)
            .SetValidator(validator);

        RuleFor(x => x)
            .CustomAsync(ValidateCountryDependentFieldsAsync);
    }

    private async Task ValidateCountryDependentFieldsAsync(
        UpdatePersonalInformationCommand command,
        ValidationContext<UpdatePersonalInformationCommand> context,
        CancellationToken cancellationToken)
    {
        var request = command.Model;

        var hasPhoneNumber =
            !string.IsNullOrWhiteSpace(request.PhoneNumber);

        var hasZipCode =
            !string.IsNullOrWhiteSpace(request.ZipCode);

        if (!request.CountryId.HasValue)
            return;

        var country = await _countryRepository.GetAsync(
            new GetCountryByIdSpecification(request.CountryId.Value),
            cancellationToken);

        if (country is null)
        {
            context.AddFailure(
                "Model.CountryId",
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
                "Model.PhoneNumber",
                "Phone number does not match the selected country's format.");
        }

        if (hasZipCode &&
            !IsValidZipCode(
                request.ZipCode!,
                country.ZipCodeMask))
        {
            context.AddFailure(
                "Model.ZipCode",
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

        var numberWithoutCountryCode =
            phoneNumber[phoneCode.Length..];

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
}