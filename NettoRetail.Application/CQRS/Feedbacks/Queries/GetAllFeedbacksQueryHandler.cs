using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Feedbacks.Queries;

public record GetAllFeedbacksQuery(PaginationRequest PageModel) : IRequest<PaginatedResponse<FeedbackResponse>>;

public class GetAllFeedbacksQueryHandler(IFeedbackRepository feedbackRepository)
   : IRequestHandler<GetAllFeedbacksQuery, PaginatedResponse<FeedbackResponse>>
{
  public async Task<PaginatedResponse<FeedbackResponse>> Handle(GetAllFeedbacksQuery request, CancellationToken cancellationToken)
  {
    var specBuilder = new SpecificationBuilder<FeedbackEntity>();
    var skip = (request.PageModel.PageNumber - 1) * request.PageModel.PageSize;

    var specification = specBuilder
      .AddOrderBy(f => f.DateSubmitted, isDesc: true)
      .Skip(skip)
      .Take(request.PageModel.PageSize)
      .Build();

    var (items, totalAmount) = await feedbackRepository.GetAllPaginatedAsync(specification, cancellationToken);
    var data = items.Adapt<List<FeedbackResponse>>();

    var responeModel = new PaginatedResponse<FeedbackResponse> {
      Items = data,
      TotalAmount = totalAmount
    };

    return responeModel;
  }
}