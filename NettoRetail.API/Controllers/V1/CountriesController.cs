using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Countries.Queries;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class CountriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllCountries(CancellationToken cancellationToken)
    {
        var getAllCountriesQuery = new GetAllCountriesQuery();
        var data = await mediator.Send(getAllCountriesQuery, cancellationToken);

        var response = new ApiResponse<IEnumerable<CountryResponse>>(data);

        return Ok(response);
    }
}
