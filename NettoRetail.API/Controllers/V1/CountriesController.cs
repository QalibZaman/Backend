using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.CQRS.Countries.Queries;
using NettoRetail.Application.CQRS.Shops.Queries;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class CountriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllCountries()
    {
        var getAllCountriesQuery = new GetAllCountriesQuery();
        var response = await mediator.Send(getAllCountriesQuery);

        return Ok(response);
    }
}
