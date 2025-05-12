using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Brands.Commands;
using NettoRetail.Application.CQRS.Payments.Commands;
using NettoRetail.Application.Models.Payments;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1
{
    [ApiController]
    [ApiVersion(1)]
    [Route("/api/v{version:apiVersion}/[controller]")]
    public class PaymentsController(IMediator mediator) : ControllerBase
    {
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPayment(AddPaymentRequest model, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();

            var command = new AddPaymentCommand(userId, model.PaymentMethodId);

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [Authorize]
        [HttpPut("{id}")]

        public async Task<IActionResult> EditPayment(Guid id, [FromBody] EditPaymentRequest model, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();

            if (id == Guid.Empty)
            {
                return BadRequest("Invalid payment method ID.");
            }

            var command = new EditPaymentCommand(userId, id, model);

            var result = await mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{paymentMethodId}")]
        public async Task<IActionResult> DeletePayment(string paymentMethodId, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();

            var command = new DeletePaymentCommand(userId, paymentMethodId);

            var response = await mediator.Send(command, cancellationToken);

            return Ok(new ApiResponse<PaymentMethodDeleteResponse>(response));
        }
    }
}
