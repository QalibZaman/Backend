using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Customers;
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
}