using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.ICountryCacheService;

public class CountryCacheService : ICountryCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IServiceScopeFactory _scopeFactory;

    public CountryCacheService(IMemoryCache memoryCache, IServiceScopeFactory scopeFactory)
    {
        _memoryCache = memoryCache;
        _scopeFactory = scopeFactory;
    }

    public async Task<IEnumerable<CountryEntity>> GetCountriesAsync(CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(CacheServiceKeys.COUNTRIES_KEY, out IEnumerable<CountryEntity>? countries))
        {
            return countries!;
        }

        await LoadCountriesIntoCacheAsync(cancellationToken);
        return _memoryCache.Get<IEnumerable<CountryEntity>>(CacheServiceKeys.COUNTRIES_KEY) ?? Enumerable.Empty<CountryEntity>();
    }

    private async Task LoadCountriesIntoCacheAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var countryRepository = scope.ServiceProvider.GetRequiredService<ICountryRepository>();

        var countries = await countryRepository.GetAllAsync(cancellationToken);

        _memoryCache.Set(CacheServiceKeys.COUNTRIES_KEY, countries, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        });
    }
}
