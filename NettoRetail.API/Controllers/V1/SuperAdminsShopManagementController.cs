using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Attributes;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Reviews.Commands;
using NettoRetail.Application.CQRS.Reviews.Queries;
using NettoRetail.Application.CQRS.SuperAdminsShopManagement.Commands;
using NettoRetail.Application.CQRS.SuperAdminsShopManagement.Queries;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Models.ClientsShop;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Review;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/super-admins")]
public class SuperAdminsShopManagementController(IMediator mediator) : ControllerBase
{
    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateShopForClient(ClientsShopModel shop)
    {
        var createShopCommand = new CreateShopForClientCommand(shop);
        var data = await mediator.Send(createShopCommand);

        var response = new ApiResponse<ShopResponse>(data);

        return Created(string.Empty, response);
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpGet("{shopId:int}")]
    public async Task<IActionResult> GetClientsShopInfo(int shopId)
    {
        var getClientsShopInfoQuery = new GetClientsShopInfoQuery(shopId);
        var data = await mediator.Send(getClientsShopInfoQuery);

        var response = new ApiResponse<ClientsShopInfoResponse>(data);

        return Ok(response);
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpGet("shops")]
    public async Task<IActionResult> GetAllShopsList([FromQuery] PaginationRequest pageModel)
    {
        var getAllShopsInfoQuery = new GetAllShopsInfoQuery(pageModel);
        var data = await mediator.Send(getAllShopsInfoQuery);

        var response = new ApiResponse<PaginatedResponse<AllShopsInfoResponse>>(data);

        return Ok(response);
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews([FromQuery] string? shopName, int? rating, ReviewStatusEnum? status, DateRangeFilterEnum dateRange, CancellationToken cancellationToken)
    {
        var query = new GetReviewsQuery(shopName, rating, status, dateRange);
        var reviews = await mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<List<ReviewResponse>>(reviews));
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPatch("reviews/{reviewId}/approve")]
    public async Task<IActionResult> ApproveReview(Guid reviewId, CancellationToken cancellationToken)
    {
        await mediator.Send(new ApproveReviewCommand(reviewId), cancellationToken);
        return NoContent();
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPost("reviews/bulk-approve")]
    public async Task<IActionResult> BulkApproveReviews([FromBody] List<Guid> reviewIds, CancellationToken cancellationToken)
    {
        var query = new BulkApproveReviewsCommand(reviewIds);
        await mediator.Send(query, cancellationToken);
        return NoContent();
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPatch("reviews/{reviewId}/reject")]
    public async Task<IActionResult> RejectReview(Guid reviewId, [FromBody] RejectReviewRequest request, CancellationToken cancellationToken)
    {
        var query = new RejectReviewCommand(reviewId, request.Reason);
        await mediator.Send(query, cancellationToken);
        return NoContent();
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPatch("reviews/bulk-reject")]
    public async Task<IActionResult> BulkRejectReviews([FromBody] BulkRejectReviewsRequest request, CancellationToken cancellationToken)
    {
        var query = new BulkRejectReviewsCommand(request.ReviewIds, request.Reason);
        await mediator.Send(query, cancellationToken);
        return NoContent();
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpDelete("reviews/{reviewId}")]
    public async Task<IActionResult> DeleteReview(Guid reviewId, CancellationToken cancellationToken)
    {
        var query = new DeleteReviewCommand(reviewId);
        await mediator.Send(query, cancellationToken);
        return NoContent();
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpDelete("reviews/bulk-delete")]
    public async Task<IActionResult> BulkDeleteReviews([FromBody] BulkDeleteReviewsRequest request, CancellationToken cancellationToken)
    {
        var query = new BulkDeleteReviewsCommand(request.ReviewIds);
        await mediator.Send(query, cancellationToken);
        return NoContent();
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpGet("reviews/{reviewId}")]
    public async Task<IActionResult> GetReviewDetail(Guid reviewId, CancellationToken cancellationToken)
    {
        var query = new GetReviewDetailQuery(reviewId);
        var response = await mediator.Send(query, cancellationToken);
        var result = new ApiResponse<ReviewDetailResponse>(response);
        return Ok(result);
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPost("platform-admins")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePlatformAdminRequest request,
        CancellationToken cancellationToken)
    {
        var nameId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(nameId) || !Guid.TryParse(nameId, out var superAdminId))
        {
            return Unauthorized(new { message = "Missing or invalid subject claim." });
        }

        var response = await mediator.Send(
            new CreatePlatformAdminCommand(request, superAdminId),
            cancellationToken);

        return Ok(new ApiResponse<CreatePlatformAdminResponse>(response));
    }


    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPost("platform-admins/{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ActivatePlatformAdmin(Guid id)
    {
        var currentAdminId = HttpContext.GetUserId();

        var activatePlatformAdminCommand = new ActivatePlatfromAdminCommand(id, currentAdminId);
        await mediator.Send(activatePlatformAdminCommand);

        return NoContent();
    }

    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPost("platform-admins/{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeactivatePlatformAdmin(Guid id)
    {
        var currentAdminId = HttpContext.GetUserId();

        var deactivatePlatfromAdminCommand = new DeactivatePlatfromAdminCommand(id, currentAdminId);
        await mediator.Send(deactivatePlatfromAdminCommand);

        return NoContent();
    }
}