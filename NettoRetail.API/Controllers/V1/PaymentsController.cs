using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Payments.Commands;
using NettoRetail.Application.Models.Payments;

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

            var command = new AddPaymentCommand(userId, model.Token);

            var result = await mediator.Send(command, cancellationToken);

            return Ok(new ApiResponse<PaymentResponse>(result));
        }
    }
}
