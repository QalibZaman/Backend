using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Address.Commands;
using NettoRetail.Application.CQRS.Address.Queries;
using NettoRetail.Application.Models.Request.Address;
using NettoRetail.Application.Models.Response.Address;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Authorize(Policy = PolicyNames.CustomerOnlyPolicy)]
[Route("api/v{v:ApiVersion}/[controller]")]
public class AddressController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAddress(CreateAddressRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        await mediator.Send(new CreateAddressCommand(userId, request), cancellationToken);

        return Created();
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAddress(Guid id, UpdateAddressRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new UpdateAddressCommand(id, userId, request);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAddress(Guid id, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new DeleteAddressCommand(id, userId);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IList<AddressResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAddresses(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var query = new GetAddressesQuery(userId);
        var addresses = await mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<IList<AddressResponse>>(addresses));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AddressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAddress(Guid id, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var query = new GetAddressByIdAndUserIdQuery(id, userId);

        var address = await mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<AddressResponse>(address));
    }
}
