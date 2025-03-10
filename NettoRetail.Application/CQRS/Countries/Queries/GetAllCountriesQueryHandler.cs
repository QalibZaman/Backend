using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Countries.Queries;

public record GetAllCountriesQuery() : IRequest<IEnumerable<CountryResponse>>;

public class GetAllCountriesQueryHandler(ICountryRepository countryRepository) : IRequestHandler<GetAllCountriesQuery, IEnumerable<CountryResponse>>
{
    public async Task<IEnumerable<CountryResponse>> Handle(GetAllCountriesQuery request, CancellationToken cancellationToken)
    {
        var countries = await countryRepository.GetAllAsync(cancellationToken);
        var countriesResponse = countries.Adapt<IEnumerable<CountryResponse>>();

        return countriesResponse;
    }
}
