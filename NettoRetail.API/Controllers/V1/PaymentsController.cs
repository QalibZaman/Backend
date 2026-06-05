using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Payments.Commands;
using NettoRetail.Application.CQRS.Payments.Queries;
using NettoRetail.Application.Models.Payments;
using NettoRetail.Application.Models.Request;
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
        [HttpGet("history")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PaymentHistoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPaymentHistory(
            [FromQuery] PaginationRequest pageModel,
            [FromQuery] Guid accountId,
            [FromQuery] bool? sortDescending,
            CancellationToken cancellationToken)
        {
            var roleId = HttpContext.GetRoleId().ToString();

            if (roleId != SuperAdminConstants.SuperAdminRoleId)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var query = new GetPaymentHistoryQuery(pageModel, accountId, sortDescending);

            var result = await mediator.Send(query, cancellationToken);

            return Ok(new ApiResponse<PaginatedResponse<PaymentHistoryResponse>>(result));
        }

        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddPayment([FromBody] AddPaymentRequest model, CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();
            var userId = HttpContext.GetUserId();

            var command = new AddPaymentCommand(userId, shopId, model.PaymentMethodId);

            await mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [Authorize]
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<PaymentHistoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> EditPayment([FromBody] EditPaymentRequest model, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();

            if (model.StripePaymentMethodId == string.Empty)
                return BadRequest(new ApiResponse<string>("Invalid payment method ID."));

            var command = new EditPaymentCommand(userId, model);

            var result = await mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{paymentMethodId}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentMethodDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePayment(string paymentMethodId, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var shopid = HttpContext.GetUserShopId();

            var command = new DeletePaymentCommand(userId, paymentMethodId, shopid);

            var response = await mediator.Send(command, cancellationToken);

            return Ok(new ApiResponse<PaymentMethodDeleteResponse>(response));
        }

        [Authorize]
        [HttpPut("{stripePaymentMethodId}/set-default")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeDefaultPayment(string stripePaymentMethodId, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();

            var command = new ChangeDefaultPaymentCommand(stripePaymentMethodId, userId);

            await mediator.Send(command, cancellationToken);

            return NoContent();
        }
        
        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<PaymentMethodsResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ViewPayemntMethods(CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();
            var userId = HttpContext.GetUserId();

            var command = new GetPaymentMethodsQuery(userId, shopId);
            var result = await mediator.Send(command, cancellationToken);

            return Ok(new ApiResponse<List<PaymentMethodsResponse>>(result));
        }
    }
}
