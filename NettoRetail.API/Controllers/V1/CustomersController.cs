using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Customers.Commands;
using NettoRetail.Application.CQRS.Customers.Queries;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Customer;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<CreateCustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest customer, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var command = new CreateCustomerCommand(shopId, customer);
        var result = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<CreateCustomerResponse>(result);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("sign-up/{shopId:int}")]
    [ProducesResponseType(typeof(ApiResponse<SignUpCustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SignUpCustomer(int shopId, [FromBody] SignUpCustomerRequest customer, CancellationToken cancellationToken)
    {
        var command = new SignUpCustomerCommand(shopId, customer);
        var result = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<SignUpCustomerResponse>(result);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("log-in/{shopId:int}")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LogInCustomer([FromBody] LoginModel model, int shopId, CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        ip = ip.Trim();
        var userAgent = Request.Headers.UserAgent.ToString();

        var command = new LogInCustomerCommand(model, ip, userAgent, shopId);

        var data = await mediator.Send(command, cancellationToken);
        if (data == null)
            return Unauthorized(new ApiResponse("Invalid credentials"));
        return Ok(new ApiResponse<TokenResponse>(data));
    }

    [AllowAnonymous]
    [HttpGet("verify-customer-activation")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyCustomer([FromQuery] string token, CancellationToken cancellationToken)
    {
        var command = new VerifyCustomerEmailCommand(token);
        var tokenResponse = await mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<string>("Your email was successfully verified!"));
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<CustomerResponse>>), StatusCodes.Status200OK)]

    public async Task<IActionResult> GetCustomers([FromQuery] PaginationRequest pageModel, [FromQuery] GetCustomersRequest? filter, CancellationToken cancellationToken)
    {
        var query = new GetCustomersQuery(pageModel, filter);
        var result = await mediator.Send(query, cancellationToken);
        var response = new ApiResponse<PaginatedResponse<CustomerResponse>>(result);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("{loyaltyId}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerInfo(string loyaltyId, CancellationToken cancellationToken)
    {
        var query = new GetCustomerInfoQuery(loyaltyId);
        var result = await mediator.Send(query, cancellationToken);
        var response = new ApiResponse<CustomerInfoResponse>(result);
        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("{accountId:guid}/contact-info")]
    [ProducesResponseType(typeof(ApiResponse<CustomerContactInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerContactInfo([FromRoute] Guid accountId,CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var query = new GetCustomerContactInfoQuery(accountId,shopId);
        var result = await mediator.Send(query, cancellationToken);
        var response = new ApiResponse<CustomerContactInfoResponse>(result);
        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("{customerId:guid}/orders")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<CustomerOrderHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse),StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrdersFortheCustomer([FromRoute] Guid customerId,[FromQuery]PaginationRequest paginationRequest,CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var query = new GetOrdersForTheCustomeQuery(shopId,customerId,paginationRequest);
        var result = await mediator.Send(query,cancellationToken);
        var response = new ApiResponse<PaginatedResponse<CustomerOrderHistoryResponse>>(result);
        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
    [HttpPut("{loyaltyId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCustomer(string loyaltyId, [FromBody] UpdateCustomerRequest model, CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(loyaltyId, model);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.CustomerOnlyPolicy)]
    [HttpPut("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new ChangeCustomerPasswordCommand(userId, request);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var command = new DeleteCustomerCommand(id, shopId);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("send-reset-password-email")]
    [ProducesResponseType(typeof(ApiResponse<SendResetPasswordLinkResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendResetPasswordLink(
        [FromBody]SendResetPasswordLinkRequest request,
        CancellationToken cancellationToken)
    {
        var query = new SendResetPasswordLinkCommand(request);
        var result = await mediator.Send(query, cancellationToken);
        var response = new ApiResponse<SendResetPasswordLinkResponse>(result);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("reset-password-token-validation")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetCustomerPasswordVerification(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var query = new ResetCustomerPasswordVerificationCommand(token);
        var result = await mediator.Send(query, cancellationToken);
        var response = new ApiResponse<string>(result);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPatch("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetCustomerPassword(
        [FromBody] ResetCustomerPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var query = new ResetCustomerPasswordCommand(request);
        await mediator.Send(query, cancellationToken);

        return NoContent();
    }
}
