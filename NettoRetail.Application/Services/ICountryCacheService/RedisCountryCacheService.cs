using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using StackExchange.Redis;

namespace NettoRetail.Application.Services.ICountryCacheService;

public class RedisCountryCacheService : ICountryCacheService
{
    private readonly IDistributedCache _redisCache;
    private readonly IServiceScopeFactory _scopeFactory;

    public RedisCountryCacheService(IDistributedCache redisCache, IServiceScopeFactory scopeFactory)
    {
        _redisCache = redisCache;
        _scopeFactory = scopeFactory;
    }

    public async Task<IEnumerable<CountryEntity>> GetCountriesAsync(CancellationToken cancellationToken)
    {
        var countriesString = await _redisCache.GetStringAsync(CacheServiceKeys.COUNTRIES_KEY, cancellationToken);
        if (!countriesString.IsNullOrEmpty())
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(countriesString!));
            return await JsonSerializer.DeserializeAsync<IEnumerable<CountryEntity>>(stream, cancellationToken: cancellationToken)
                ?? new List<CountryEntity>();
        }

        return await LoadCountriesIntoCacheAsync(cancellationToken);
    }

    private async Task<IEnumerable<CountryEntity>> LoadCountriesIntoCacheAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var countryRepository = scope.ServiceProvider.GetRequiredService<ICountryRepository>();

        var countries = await countryRepository.GetAllAsync(cancellationToken);

        await _redisCache.SetAsync(CacheServiceKeys.COUNTRIES_KEY, JsonSerializer.SerializeToUtf8Bytes(countries));
        return countries;
    }
}


public class Redis2CountryCacheService : ICountryCacheService
{
    private readonly IDatabase _redisDb;
    private readonly IServiceScopeFactory _scopeFactory;

    public Redis2CountryCacheService(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory)
    {
        _redisDb = redis.GetDatabase();
        _scopeFactory = scopeFactory;
    }

    public async Task<IEnumerable<CountryEntity>> GetCountriesAsync(CancellationToken cancellationToken)
    {
        var cachedData = await _redisDb.StringGetAsync(CacheServiceKeys.COUNTRIES_KEY);
        if (cachedData.HasValue)
        {
            return JsonSerializer.Deserialize<IEnumerable<CountryEntity>>(cachedData!)!;
        }

        await LoadCountriesIntoCacheAsync(cancellationToken);

        cachedData = await _redisDb.StringGetAsync(CacheServiceKeys.COUNTRIES_KEY);
        return cachedData.HasValue
            ? JsonSerializer.Deserialize<IEnumerable<CountryEntity>>(cachedData!)!
            : Enumerable.Empty<CountryEntity>();
    }

    private async Task LoadCountriesIntoCacheAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var countryRepository = scope.ServiceProvider.GetRequiredService<ICountryRepository>();

        var countries = await countryRepository.GetAllAsync(cancellationToken);

        var jsonData = JsonSerializer.Serialize(countries);
        await _redisDb.StringSetAsync(
            CacheServiceKeys.COUNTRIES_KEY,
            jsonData,
            TimeSpan.FromHours(24)
        );
    }
}