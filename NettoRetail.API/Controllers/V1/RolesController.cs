using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Controllers.V1
{
    [ApiVersion(1)]
    [Route("/api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class RolesController(IRoleRepository roleRepository) : ControllerBase
    {
        [HttpGet]
        public IActionResult AddRole()
        {
            return Ok();
        }
    }
}
