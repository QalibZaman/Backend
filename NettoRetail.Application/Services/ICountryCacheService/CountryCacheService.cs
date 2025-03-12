using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.ICountryCacheService;

public class CountryCacheService : ICountryCacheService
{
    private const string CacheKey = "Countries";
    private readonly IMemoryCache _memoryCache;
    private readonly IServiceScopeFactory _scopeFactory;

    public CountryCacheService(IMemoryCache memoryCache, IServiceScopeFactory scopeFactory)
    {
        _memoryCache = memoryCache;
        _scopeFactory = scopeFactory;
    }

    public async Task<IEnumerable<CountryEntity>> GetCountriesAsync(CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(CacheKey, out IEnumerable<CountryEntity>? countries))
        {
            return countries!;
        }

        await LoadCountriesIntoCacheAsync(cancellationToken);
        return _memoryCache.Get<IEnumerable<CountryEntity>>(CacheKey) ?? Enumerable.Empty<CountryEntity>();
    }

    private async Task LoadCountriesIntoCacheAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var countryRepository = scope.ServiceProvider.GetRequiredService<ICountryRepository>();

        var countries = await countryRepository.GetAllAsync(cancellationToken);

        _memoryCache.Set(CacheKey, countries, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        });
    }
}
