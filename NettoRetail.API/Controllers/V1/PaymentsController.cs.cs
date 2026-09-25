using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Payments.Commands;
using NettoRetail.Application.Models.Payments;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1
{
    [ApiController]
    [ApiVersion(1)]
    [Route("/api/v{version:apiVersion}/[controller]")]
    public class PaymentsController(IMediator mediator) : ControllerBase
    {
        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddPayment([FromBody] AddPaymentRequest model, CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();
            var userId = HttpContext.GetUserId();

            var command = new AddPaymentCommand(userId, shopId, model.PaymentMethodId);

            await mediator.Send(command, cancellationToken);

            return NoContent();
        }
    }
}