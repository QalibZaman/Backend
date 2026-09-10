using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Users.Commands.PersonalInformation;
using NettoRetail.Application.CQRS.Users.Queries.PersonalInformation;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.API.Controllers
{
    [ApiController]
    [ApiVersion(1)]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController(IMediator mediator) : ControllerBase
    {
        [HttpGet("personal-information")]
        public async Task<IActionResult> GetPersonalInformation(
            CancellationToken cancellationToken)
        {
            var query = new GetPersonalInformationQuery();
            var data = await mediator.Send(query, cancellationToken);
            var response = new ApiResponse<GetPersonalInformationResponse>(data);

            return Ok(response);
        }

        [HttpPut("personal-information")]
        public async Task<IActionResult> UpdatePersonalInformation([FromBody] UpdatePersonalInformationRequest request, CancellationToken cancellationToken)
        {
            var command =
                new UpdatePersonalInformationCommand(request);

            await mediator.Send(
                command,
                cancellationToken);

            return NoContent();
        }
    }
}
