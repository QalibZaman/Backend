using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Attributes;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Feedback.Queries;
using NettoRetail.Application.CQRS.Feedbacks.Commands;
using NettoRetail.Application.CQRS.Feedbacks.Queries;
using NettoRetail.Application.CQRS.Reviews.Commands;
using NettoRetail.Application.Models.Feedback;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Feedback;
using NettoRetail.Application.Models.Request.Review;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class FeedbacksController(IMediator mediator) : ControllerBase
{

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FeedbackResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateFeedback(CreateFeedbackModel model, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new CreateFeedbackCommand(model, userId);
        var data = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<FeedbackResponse>(data);
        return Ok(response);
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<FeedbackResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    public async Task<IActionResult> GetFeedbacks([FromQuery] GetFilteredFeedbacksModel filterModel, [FromQuery] PaginationRequest pageModel, CancellationToken cancellationToken)
    {
        var query = new GetFilteredFeedbacksQuery(filterModel, pageModel);
        var data = await mediator.Send(query, cancellationToken);
        return Ok(new ApiResponse<PaginatedResponse<FeedbackResponse>>(data));
    }

    [Authorize]
    [HttpDelete("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    public async Task<IActionResult> DeleteFeedback(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteFeedbackCommand(id), cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpDelete("bulk-delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    public async Task<IActionResult> BulkDeleteFeedbacks([FromBody] BulkDeleteFeedbacksRequest request, CancellationToken cancellationToken)
    {
        var command = new BulkDeleteFeedbacksCommand(request.feedbacksIds);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFeedbackDetail(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetFeedbackDetailQuery(id);

        var response = await mediator.Send(query, cancellationToken);

        var result = new ApiResponse<FeedbackDetailResponse>(response);
        return Ok(result);
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPatch("{id:Guid}/approve")]
    public async Task<IActionResult> ApproveFeedback(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new ApproveFeedbackCommand(id), cancellationToken);
        return NoContent();
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPatch("{id:Guid}/reject")]
    public async Task<IActionResult> RejectFeedback(Guid id, [FromBody] RejectFeedbackRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new RejectFeedbackCommand(id, request.Reason), cancellationToken);
        return NoContent();
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPatch("bulk-approve")]
    public async Task<IActionResult> BulkApproveFeedbacks([FromBody] List<Guid> ids, CancellationToken cancellationToken)
    {
        var command = new BulkApproveFeedbacksCommand(ids);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPatch("bulk-reject")]
    public async Task<IActionResult> BulkRejectFeedbacks([FromBody] BulkRejectFeedbacksRequest request, CancellationToken cancellationToken)
    {
        var command = new BulkRejectFeedbacksCommand(request.FeedBackIds, request.Reason);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPatch("{id:Guid}/toggle-visibility")]
    public async Task<IActionResult> ToggleVisibilityFeedbacks(Guid id, [FromBody] ToggleVisibilityRequest request, CancellationToken cancellationToken)
    {
        var command = new ToggleVisibilityCommand(id, request.IsVisible);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
