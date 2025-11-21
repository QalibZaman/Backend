using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Countries.Queries;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class CountriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CountryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCountries(CancellationToken cancellationToken)
    {
        var query = new GetAllCountriesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Ok(new ApiResponse<IEnumerable<CountryResponse>>(result));
    }
}
