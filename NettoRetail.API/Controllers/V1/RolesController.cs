using Asp.Versioning;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Roles;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Controllers.V1
{
    [ApiVersion(1)]
    [Route("/api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class RolesController(IRoleRepository roleRepository, IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddRole(CreateRoleRequest model, CancellationToken cancellationToken)
        {
            var command = model.Adapt<CreateRoleCommand>();

            var role = await mediator.Send(command, cancellationToken);

            return Ok(new ApiResponse<CreateRoleResponse>(role));
        }
    }
}
