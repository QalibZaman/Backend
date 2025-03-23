using NettoRetail.Application.Models.Country;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.ICountryValidationService;

public interface ICountryDetailsValidationService
{
    Task ValidateAsync(CountryValidationModel shop, CancellationToken cancellationToken);
    bool IsPhoneNumberValid(CountryEntity country, string phoneNumber);
}
