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
    specBuilder.Add<GetFeedbacksByAccountIdsSpecification>(request.Model.AccountIds);

    specBuilder.AddOrderBy(f => f.DateSubmitted);
    specBuilder.AddInclude(f => f.Account);

    var specification = specBuilder.Skip(skip).Take(request.PageModel.PageSize).Build();

    var (items, totalAmount) = await feedbackRepository.GetAllPaginatedAsync(specification, cancellationToken);

    var data = items.Select(f => new FeedbackResponse
    {
      Id = f.Id,
      Rating = f.Rating,
      Title = f.Title,
      Text = f.Text,
      IsPublic = f.IsPublic,
      StaffName = f.Account?.FullName ?? "Anonymous",
      Role = f.Account?.Role,
      DateSubmitted = f.DateSubmitted,
      Status = f.Status.ToString(),
      ShopName = f.Account?.Shop?.Name
    }).ToList();

    return new PaginatedResponse<FeedbackResponse>
    {
      Items = data,
      TotalAmount = totalAmount
    };
  }
}