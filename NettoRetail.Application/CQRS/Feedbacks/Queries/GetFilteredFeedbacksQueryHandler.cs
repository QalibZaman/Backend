using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Feedback;
using NettoRetail.Application.Models.Response;
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
    var skip = (request.PageModel.PageNumber - 1) * request.PageModel.PageSize;

    specBuilder.Add<GetFeedbacksByStatusesSpecification>(request.Model.Statuses);
    specBuilder.Add<GetFeedbacksByDateRangeSpecification>(request.Model.StartDate, request.Model.EndDate);
    specBuilder.Add<GetFeedbacksByRatingsSpecification>(request.Model.Ratings);
    specBuilder.Add<GetFeedbacksByShopNamesSpecification>(request.Model.ShopName);
    specBuilder.Add<GetFeedbacksByPublicitySpecification>(request.Model.IsPublic);
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