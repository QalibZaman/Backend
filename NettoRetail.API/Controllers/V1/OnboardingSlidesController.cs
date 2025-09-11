using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.OnboardingSlides.Commands;
using NettoRetail.Application.CQRS.OnboardingSlides.Queries;
using NettoRetail.Application.Models.Request.OnboardingSlides;
using NettoRetail.Application.Models.Response.OnboardingSlides;

namespace NettoRetail.API.Controllers.V1
{
    [ApiController]
    [ApiVersion(1)]
    [Route("/api/v{version:apiVersion}/onboarding-slides")]
    [Authorize(Policy = "SuperAdminPolicy")]

    public class OnboardingSlidesController(IMediator mediator) : ControllerBase
    {
        [HttpGet("all")]
        [ProducesResponseType(typeof(ApiResponse<List<OnboardingSlideResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllSlides(CancellationToken cancellationToken)
        {
            var data = await mediator.Send(new GetOnboardingSlidesQuery(), cancellationToken);

            return Ok(new ApiResponse<List<OnboardingSlideResponse>>(data));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<OnboardingSlideResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSlideById(int id, CancellationToken cancellationToken)
        {
            var data = await mediator.Send(new GetOnboardingSlideByIdQuery(id), cancellationToken);

            return Ok(new ApiResponse<OnboardingSlideResponse>(data));
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(ApiResponse<OnboardingSlideResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateSlide([FromBody] CreateOnboardingSlideRequest request, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var data = await mediator.Send(new CreateOnboardingSlideCommand(request, userId), cancellationToken);

            return Created(string.Empty, new ApiResponse<OnboardingSlideResponse>(data));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<OnboardingSlideResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSlide(int id, [FromBody] UpdateOnboardingSlideRequest request, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var data = await mediator.Send(new UpdateOnboardingSlideCommand(id, request, userId), cancellationToken);

            return Ok(new ApiResponse<OnboardingSlideResponse>(data));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSlide(int id, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            await mediator.Send(new DeleteOnboardingSlideCommand(id, userId), cancellationToken);

            return NoContent();
        }

        [HttpPost("reorder")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReorderSlides([FromBody] ReorderOnboardingSlidesRequest request, CancellationToken cancellationToken)
        {
            await mediator.Send(new ReorderOnboardingSlidesCommand(request), cancellationToken);

            return NoContent();
        }

        [HttpPost("draft")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveDraft([FromBody] CreateOrUpdateOnboardingSlideDraftRequest request, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            await mediator.Send(new CreateOrUpdateOnboardingSlideDraftCommand(request, userId), cancellationToken);
            return NoContent();
        }

        [HttpGet("draft/{slideId:int?}")]
        [ProducesResponseType(typeof(ApiResponse<OnboardingSlideDraftResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDraft(int? slideId, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var data = await mediator.Send(new GetOnboardingSlideDraftQuery(slideId, userId), cancellationToken);
            return Ok(new ApiResponse<OnboardingSlideDraftResponse>(data));
        }

        [HttpDelete("draft/{slideId:int?}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDraft(int? slideId, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            await mediator.Send(new DeleteOnboardingSlideDraftCommand(slideId, userId), cancellationToken);
            return NoContent();
        }
    }
}