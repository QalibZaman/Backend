using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.ICountryCacheService;

public interface ICountryCacheService
{
    Task<IEnumerable<CountryEntity>> GetCountriesAsync(CancellationToken cancellationToken);
}
