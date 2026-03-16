using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Feedback;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IFeedbackService;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Feedbacks.Queries;

public record GetFilteredFeedbacksQuery(GetFilteredFeedbacksModel Model, PaginationRequest PageModel) : IRequest<PaginatedResponse<FeedbackResponse>>;

public class GetFilteredFeedbacksQueryHandler(
  IFeedbackRepository feedbackRepository,
  IFilteredFeedbackService filteredFeedbackService)
   : IRequestHandler<GetFilteredFeedbacksQuery, PaginatedResponse<FeedbackResponse>>
{
  public async Task<PaginatedResponse<FeedbackResponse>> Handle(GetFilteredFeedbacksQuery request, CancellationToken cancellationToken)
  {
    var specification = filteredFeedbackService.BuildFilteredFeedbacksSpecification(request.Model, request.PageModel);

    var (items, totalAmount) = await feedbackRepository.GetAllPaginatedAsync(specification, cancellationToken);
    var data = items.Adapt<List<FeedbackResponse>>();

    var responseModel = new PaginatedResponse<FeedbackResponse>
    {
      Items = data,
      TotalAmount = totalAmount
    };

    return responseModel;
  }
}
