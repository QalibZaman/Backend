using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.AdminProductReviews.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.AdminProductReview;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;
[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
public class AdminProductReviewController(IMediator mediator) : ControllerBase
{
    [HttpGet("{inventoryItemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AdminProductReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProductReviews(
        [FromQuery] PaginationRequest pagination,
        Guid inventoryItemId,
        CancellationToken cancellationToken)
    {
        var query = new GetProductReviewsQuery(pagination, inventoryItemId);
        var response = await mediator.Send(query, cancellationToken);
        return Ok(new ApiResponse<PaginatedResponse<AdminProductReviewResponse>>(response));
    }
}