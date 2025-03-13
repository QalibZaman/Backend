using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Shop;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.ICountryValidationService;

public class CountryDetailsValidationService(ICountryRepository countryRepository) : ICountryDetailsValidationService
{
    public async Task ValidateAsync(ShopModel shop, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        var specification = new GetCountryByIdSpecification(shop.CountryId);
        var country = await countryRepository.GetAsync(specification, cancellationToken);

        if (country == null)
        {
            errors.Add("Invalid country id");
        }
        else
        {
            if (!IsPhoneNumberValid(country, shop.PhoneNumber))
            {
                errors.Add("Invalid phone number");
            }

            if (!IsZipCodeValid(country, shop.ZipCode))
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
