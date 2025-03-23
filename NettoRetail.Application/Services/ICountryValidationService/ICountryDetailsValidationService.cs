using NettoRetail.Application.Models.Country;

namespace NettoRetail.Application.Services.ICountryValidationService;

public interface ICountryDetailsValidationService
{
    Task ValidateAsync(CountryValidationModel shop, CancellationToken cancellationToken);
}
