using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.ProductReviews;
using NettoRetail.Application.CQRS.ProductReviews.Commands;
using NettoRetail.Application.Models.Request;


namespace NettoRetail.API.Controllers
{

    [ApiController]
    [ApiVersion(1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class ProductReviewController(IMediator mediator) : ControllerBase
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
        [ProducesResponseType(StatusCodes.Status200OK)]
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
    }
}
