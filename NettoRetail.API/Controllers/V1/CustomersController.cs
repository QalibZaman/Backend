using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Customers.Commands;
using NettoRetail.Application.CQRS.Customers.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.API.Controllers.V1;

[ApiVersion(1)]
[ApiController]
[Route("api/v1/[controller]")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpPost("sign-up")]
    public async Task<IActionResult> Create(CreateCustomerRequest customer, CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(customer);
        var data = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<CreateCustomerResponse>(data);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pageModel, [FromQuery] GetCustomersRequest? model, CancellationToken cancellationToken)
    {
        var command = new GetCustomersQuery(pageModel, model);
        var data = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<PaginatedResponse<CustomerResponse>>(data);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("{loyaltyId}")]
    public async Task<IActionResult> GetCustomerInfo(string loyaltyId, CancellationToken cancellationToken)
    {
        var command = new GetCustomerInfoQuery(loyaltyId);
        var data = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<CustomerInfoResponse>(data);
        return Ok(response);
    }

    [Authorize]
    [HttpPut("{loyaltyId}")]
    public async Task<IActionResult> UpdateCustomer(string loyaltyId, [FromBody] UpdateCustomerRequest model, CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(loyaltyId, model);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}