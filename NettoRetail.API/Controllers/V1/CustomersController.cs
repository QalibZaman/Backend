using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Customers.Commands;
using NettoRetail.Application.CQRS.Customers.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost("sign-up")]
    [HttpPost("create-customer")]
    [ProducesResponseType(typeof(ApiResponse<CreateCustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest customer, CancellationToken cancellationToken)
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
    public async Task<IActionResult> SighnUpCustomer(int shopId, [FromBody] SignUpCustomerRequest customer, CancellationToken cancellationToken)
    {
        var command = new SighnUpCustomerCommand(shopId, customer);
        var result = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<SignUpCustomerResponse>(result);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("verify-customer-activation")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyCustomer([FromQuery] string token, CancellationToken cancellationToken)
    {
        var command = new VerifyCustomerEmailCommand(token);
        await mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<string>("Your email was successfully verified!"));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<CustomerResponse>>), StatusCodes.Status200OK)]

    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pageModel, [FromQuery] GetCustomersRequest? filter, CancellationToken cancellationToken)
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

    [Authorize]
    [HttpPut("{loyaltyId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCustomer(string loyaltyId, [FromBody] UpdateCustomerRequest model, CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(loyaltyId, model);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPatch("{id:guid}")]
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
}