using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.ProductReviews;
using NettoRetail.Application.CQRS.ProductReviews.Commands;
using NettoRetail.Application.CQRS.ProductReviews.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.ProductReview;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = PolicyNames.CustomerOnlyPolicy)]
public class CustomerProductReviewController(IMediator mediator) : ControllerBase
{
    [HttpPost("{inventoryItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateReview(
    Guid inventoryItemId,
    [FromBody] CreateProductReviewRequest request,
    CancellationToken cancellationToken)
    {
        var customerId = HttpContext.GetUserId();

        await mediator.Send(new CreateProductReviewCommand(request, customerId, inventoryItemId), cancellationToken);

        return CreatedAtAction(nameof(CreateReview), new { inventoryItemId }, null);
    }

    [HttpPost("product-review-image")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadProductReviewImage(
        IFormFile file,
        Guid productReviewId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new UploadProductReviewImageCommand(file, productReviewId), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ProductReviewResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<ProductReviewResponse>>> GetProductReviews(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var customerId = HttpContext.GetUserId();
        var query = new GetProductReviewQuery(customerId, pagination);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(new ApiResponse<PaginatedResponse<ProductReviewResponse>>(result));
    }

    [HttpGet("{productReviewId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductReviewResponse>> GetProductReviewById(
        Guid productReviewId,
        CancellationToken cancellationToken)
    {
        var customerId = HttpContext.GetUserId();
        var query = new GetProductReviewByIdQuery(productReviewId, customerId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(new ApiResponse<ProductReviewResponse>(result));
    }

    [HttpGet("{productReviewId}/image")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProductReviewImage(
        [FromRoute] string productReviewId,
        CancellationToken cancellationToken)
    {
        var customerId = HttpContext.GetUserId();
        var query = new GetProductReviewImageQuery(productReviewId, customerId);
        var imageUrl = await mediator.Send(query, cancellationToken);
        return Redirect(imageUrl);
    }
}
