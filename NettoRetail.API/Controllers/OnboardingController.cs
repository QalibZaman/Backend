using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.CQRS.Users.Commands.Onboarding;
using NettoRetail.Application.CQRS.Users.Queries.Onboarding;

namespace NettoRetail.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/users/me/onboarding")]
    public class OnboardingController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var response = await mediator.Send(
                new GetOnboardingQuery(),
                cancellationToken);

            return response is null
                ? NoContent()
                : Ok(response);
        }

        [HttpPut("complete")]
        public async Task<IActionResult> CompleteOnboarding(CancellationToken cancellationToken)
        {
            await mediator.Send(
                new CompleteOnboardingCommand(),
                cancellationToken);

            return NoContent();
        }
    }
}
