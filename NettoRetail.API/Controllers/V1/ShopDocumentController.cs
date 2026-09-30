using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.ShopDocument.Commands;
using NettoRetail.Application.CQRS.ShopDocument.Queries;
using NettoRetail.Application.Models.Request.ShopDocument;
using NettoRetail.Application.Models.Response.ShopDocument;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1
{
    [ApiController]
    [ApiVersion(1)]
    [Authorize]
    [Route("/api/v{version:apiVersion}/[controller]")]
    public class ShopDocumentController(IMediator mediator) : ControllerBase
    {
        [HttpPost("add-documents")]
        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [ProducesResponseType(typeof(ApiResponse<List<AddShopDocumentResponce>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddDocument([FromQuery] AddShopDocumentRequest request, CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();
            var command = new AddShopDocumentCommand(request, shopId);
            var result = await mediator.Send(command, cancellationToken);
            var responce = new ApiResponse<List<AddShopDocumentResponce>>(result);
            return Ok(responce);
        }

        [HttpGet("get-documents")]
        [ProducesResponseType(typeof(ApiResponse<List<GetShopDocumentResponce>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocuments([FromQuery] int shopId, CancellationToken cancellationToken)
        {
            var command = new GetShopDocumentsCommand(shopId);
            var result = await mediator.Send(command, cancellationToken);
            var responce = new ApiResponse<List<GetShopDocumentResponce>>(result);
            return Ok(responce);
        }

        [HttpDelete("delete-documents")]
        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDocuments([FromQuery] DeleteShopDocumentRequest request, CancellationToken cancellationToken)
        {
            int shopId = HttpContext.GetUserShopId();
            var command = new DeleteShopDocumentCommand(request, shopId);
            await mediator.Send(command, cancellationToken);
            return Ok();
        }
    }
}
