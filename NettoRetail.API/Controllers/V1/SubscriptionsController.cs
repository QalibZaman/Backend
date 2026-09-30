using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.SubscriptionPlans.Queries;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace NettoRetail.API.Controllers.V1
{
    [ApiController]
    [ApiVersion(1)]
    [Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
    public class SubscriptionsController(IMediator mediator) : ControllerBase
    {
        [Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
        [HttpGet("/all")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSubscriptionPlans(CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();
            var command = new GetSubscriptionsPlansQuery(shopId);

            var result = await mediator.Send(command, cancellationToken);
            var response = new ApiResponse<List<SubscriptionPlanResponce>>(result);

            return Ok(response);
        }
    }
}
