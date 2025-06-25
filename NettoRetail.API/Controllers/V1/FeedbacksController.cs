using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Feedbacks.Commands;
using NettoRetail.Application.Models.Feedback;
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
    Console.WriteLine("user id" + userId);
    var command = new CreateFeedbackCommand(model, userId);
    var data = await mediator.Send(command, cancellationToken);
    var response = new ApiResponse<FeedbackResponse>(data);
    return Created(string.Empty, response);
  }
}
