using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICountryCacheService;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Countries.Queries;

public record GetAllCountriesQuery() : IRequest<IEnumerable<CountryResponse>>;

public class GetAllCountriesQueryHandler(ICountryCacheService countryCache, IMapper mapper) : IRequestHandler<GetAllCountriesQuery, IEnumerable<CountryResponse>>
{
    public async Task<IEnumerable<CountryResponse>> Handle(GetAllCountriesQuery request, CancellationToken cancellationToken)
    {
        var countries = await countryCache.GetCountriesAsync(cancellationToken);
        var countriesResponse = mapper.Map<IEnumerable<CountryResponse>>(countries);

        return countriesResponse;
    }
}
