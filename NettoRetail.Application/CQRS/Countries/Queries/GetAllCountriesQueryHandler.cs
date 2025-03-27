using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICountryCacheService;

namespace NettoRetail.Application.CQRS.Countries.Queries;

public record GetAllCountriesQuery() : IRequest<IEnumerable<CountryResponse>>;

public class GetAllCountriesQueryHandler(ICountryCacheService countryCache) : IRequestHandler<GetAllCountriesQuery, IEnumerable<CountryResponse>>
{
    public async Task<IEnumerable<CountryResponse>> Handle(GetAllCountriesQuery request, CancellationToken cancellationToken)
    {
        var countries = await countryCache.GetCountriesAsync(cancellationToken);
        var countriesResponse = countries.Adapt<IEnumerable<CountryResponse>>();

        return countriesResponse;
    }
}
