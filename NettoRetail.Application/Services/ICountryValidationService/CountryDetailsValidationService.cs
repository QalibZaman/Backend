using System.Text.RegularExpressions;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Shop;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.ICountryValidationService;

public class CountryDetailsValidationService(ICountryRepository countryRepository) : ICountryDetailsValidationService
{
    public async Task ValidateAsync(CountryValidationModel model, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        var specification = new GetCountryByIdSpecification(model.CountryId);
        var country = await countryRepository.GetAsync(specification, cancellationToken);

        if (country == null)
        {
            errors.Add("Invalid country id");
        }
        else
        {
            if (!IsPhoneNumberValid(country, model.PhoneNumber))
            {
                errors.Add("Invalid phone number");
            }

            if (!IsZipCodeValid(country, model.ZipCode))
            {
                errors.Add("Invalid zip code");
            }
        }

        if (errors.Count > 0)
        {
            throw new BadRequestException(string.Join("; ", errors));
        }
    }

    private bool IsPhoneNumberValid(CountryEntity country, string phoneNumber)
    {
        return Regex.IsMatch(phoneNumber, country.PhoneMask);
    }

    private bool IsZipCodeValid(CountryEntity country, string zipCode)
    {
        return Regex.IsMatch(zipCode, country.ZipCodeMask);
    }
}
