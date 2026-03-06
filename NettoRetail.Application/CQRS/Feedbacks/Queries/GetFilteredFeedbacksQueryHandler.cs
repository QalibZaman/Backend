using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Feedback;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Feedbacks.Queries;

public record GetFilteredFeedbacksQuery(GetFilteredFeedbacksModel Model, PaginationRequest PageModel) : IRequest<PaginatedResponse<FeedbackResponse>>;

public class GetFilteredFeedbacksQueryHandler(IFeedbackRepository feedbackRepository)
   : IRequestHandler<GetFilteredFeedbacksQuery, PaginatedResponse<FeedbackResponse>>
{
  public async Task<PaginatedResponse<FeedbackResponse>> Handle(GetFilteredFeedbacksQuery request, CancellationToken cancellationToken)
  {
    var specBuilder = new SpecificationBuilder<FeedbackEntity>();
    var skip = PaginationHelper.CalculateSkip(request.PageModel.PageNumber, request.PageModel.PageSize);

    var statuses = request.Model.Statuses?.ToList();
    if (statuses is { Count: > 0 })
        specBuilder.Add<GetFeedbacksByStatusesSpecification>(statuses);

    if (request.Model.StartDate.HasValue && request.Model.EndDate.HasValue)
        specBuilder.Add<GetFeedbacksByDateRangeSpecification>(request.Model.StartDate.Value, request.Model.EndDate.Value);

    var ratings = request.Model.Ratings?.ToList();
    if (ratings is { Count: > 0 })
        specBuilder.Add<GetFeedbacksByRatingsSpecification>(ratings);

    if (!string.IsNullOrWhiteSpace(request.Model.ShopName))
        specBuilder.Add<GetFeedbacksByShopNamesSpecification>(request.Model.ShopName.Trim());

    var isPublic = request.Model.IsPublic?.ToList();
    if (isPublic is { Count: > 0 })
        specBuilder.Add<GetFeedbacksByPublicitySpecification>(isPublic);
    specBuilder.AddInclude(f => f.Account);
    specBuilder.AddInclude(f => f.Account.Shop!);

    var specification = specBuilder
      .AddOrderBy(f => f.DateSubmitted, isDesc: true)
      .Skip(skip)
      .Take(request.PageModel.PageSize)
      .Build();

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
