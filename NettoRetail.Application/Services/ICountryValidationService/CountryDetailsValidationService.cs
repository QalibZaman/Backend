using System.Text.RegularExpressions;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Country;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.ICountryValidationService;

public class CountryDetailsValidationService(ICountryRepository countryRepository) : ICountryDetailsValidationService
{
    public async Task ValidateAsync(CountryValidationModel model, CancellationToken cancellationToken, CountryValidationOptions? options = null)
    {
        var countryOptions = options ?? new CountryValidationOptions();
        var specification = new GetCountryByIdSpecification(model.CountryId);
        var country = await countryRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("Country not found");

        if (!IsPhoneNumberValid(country, model.PhoneNumber) && countryOptions.CheckForPhoneNumber)
        {
            throw new BadRequestException("Invalid phone number for the given country");
        }

        if (!IsZipCodeValid(country, model.ZipCode) && countryOptions.CheckForZipCode)
        {
            throw new BadRequestException("Invalid zip code for the given country");
        }
    }

    private static bool IsPhoneNumberValid(CountryEntity country, string phoneNumber)
    {
        return Regex.IsMatch(phoneNumber, country.PhoneMask);
    }

    private static bool IsZipCodeValid(CountryEntity country, string zipCode)
    {
        return Regex.IsMatch(zipCode, country.ZipCodeMask);
    }
}
