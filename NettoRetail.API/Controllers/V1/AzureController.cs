using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Azure.Queries;
using NettoRetail.Application.Models.Azure;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class AzureController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpGet("sas")]
    public async Task<IActionResult> GetSasToken([FromQuery] string fileUrl)
    {
        var shopId = HttpContext.GetUserShopId();
        var getSasTokenQuery = new GetBlobSasTokenQuery(fileUrl, shopId);

        var data = await mediator.Send(getSasTokenQuery);

        var response = new ApiResponse<SasTokenModel>(data);

        return Ok(response);
    }
}
