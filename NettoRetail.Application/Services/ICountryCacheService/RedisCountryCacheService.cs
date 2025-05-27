using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using StackExchange.Redis;

namespace NettoRetail.Application.Services.ICountryCacheService;

public class RedisCountryCacheService : ICountryCacheService
{
    private readonly IDatabase _redisDb;
    private readonly IServiceScopeFactory _scopeFactory;

    public RedisCountryCacheService(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory)
    {
        _redisDb = redis.GetDatabase();
        _scopeFactory = scopeFactory;
    }

    public async Task<IEnumerable<CountryEntity>> GetCountriesAsync(CancellationToken cancellationToken)
    {
        var hashEntries = await _redisDb.HashGetAllAsync(CacheServiceKeys.COUNTRIES_KEY);

        var isCacheComplete = await _redisDb.StringGetAsync(CacheServiceKeys.COUNTRIES_CACHE_COMPLETE_KEY);
        if (!isCacheComplete.HasValue || isCacheComplete != "true")
        {
            var countries = await LoadCountriesIntoCacheAsync(cancellationToken);
            hashEntries = await _redisDb.HashGetAllAsync(CacheServiceKeys.COUNTRIES_KEY);
            return countries;
        }

        return hashEntries
            .Select(entry => JsonSerializer.Deserialize<CountryEntity>(entry.Value!)!)
            .ToList();
    }

    public async Task<CountryEntity> GetCountryByIdAsync(int id, CancellationToken cancellationToken)
    {
        var countryValue = await _redisDb.HashGetAsync(CacheServiceKeys.COUNTRIES_KEY, id.ToString());

        if (countryValue.HasValue)
        {
            return JsonSerializer.Deserialize<CountryEntity>(countryValue!)!;
        }

        var country = await LoadCountryByIdIntoCacheAsync(id, cancellationToken);
        return country;
    }

    private async Task<IEnumerable<CountryEntity>> LoadCountriesIntoCacheAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var countryRepository = scope.ServiceProvider.GetRequiredService<ICountryRepository>();

        var countries = await countryRepository.GetAllAsync(cancellationToken);

        var entries = countries
            .Select(c => new HashEntry(c.Id.ToString(), JsonSerializer.Serialize(c)))
            .ToArray();

        await _redisDb.HashSetAsync(CacheServiceKeys.COUNTRIES_KEY, entries);
        // Set a flag to indicate full cache
        await _redisDb.StringSetAsync(CacheServiceKeys.COUNTRIES_CACHE_COMPLETE_KEY, "true");

        return countries;
    }

    private async Task<CountryEntity> LoadCountryByIdIntoCacheAsync(int id, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var countryRepository = scope.ServiceProvider.GetRequiredService<ICountryRepository>();

        var specification = new GetCountryByIdSpecification(id);
        var country = await countryRepository.GetAsync(specification, cancellationToken)
            ?? throw new ArgumentException($"Couldn't find a country with id={id}");

        var entry = new HashEntry[] { new HashEntry(country.Id.ToString(), JsonSerializer.Serialize(country)) };

        await _redisDb.HashSetAsync(CacheServiceKeys.COUNTRIES_KEY, entry);

        return country;
    }
}
