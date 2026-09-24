using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.ShopDocument.Commands;
using NettoRetail.Application.Models.Request.ShopDocument;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1
{
    [ApiController]
    [ApiVersion(1)]
    [Authorize]
    [Route("/api/v{version:apiVersion}/[controller]")]
    public class ShopDocumentController(IMediator mediator) : ControllerBase
    {
        [HttpPost("add-document")]
        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [ProducesResponseType(typeof(ApiResponse<List<ShopDocumentResponce>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddDocument(AddShopDocumentRequest request)
        {
            var shopId = HttpContext.GetUserShopId();
            var command = new AddShopDocumentCommand(request, shopId);
            var result = await mediator.Send(command);
            var responce = new ApiResponse<List<ShopDocumentResponce>>(result);
            return Ok(responce);
        }
    }
}
