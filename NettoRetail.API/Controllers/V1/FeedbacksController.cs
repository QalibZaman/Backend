using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Feedbacks.Commands;
using NettoRetail.Application.CQRS.Feedbacks.Queries;
using NettoRetail.Application.Models.Feedback;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Feedback;
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
  public async Task<IActionResult> CreateFeedback(CreateFeedbackModel model, CancellationToken cancellationToken)
  {
    var userId = HttpContext.GetUserId();
    var command = new CreateFeedbackCommand(model, userId);
    var data = await mediator.Send(command, cancellationToken);
    var response = new ApiResponse<FeedbackResponse>(data);
    return Ok(response);
  }

  [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
  [HttpGet]
  public async Task<IActionResult> GetFeedbacks([FromQuery] GetFilteredFeedbacksModel filterModel, [FromQuery] PaginationRequest pageModel, CancellationToken cancellationToken)
  {
    var query = new GetFilteredFeedbacksQuery(filterModel, pageModel);
    var data = await mediator.Send(query, cancellationToken);
    return Ok(new ApiResponse<PaginatedResponse<FeedbackResponse>>(data));
  }
}
