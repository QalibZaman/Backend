using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Feedback;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
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

    var shopName = string.IsNullOrWhiteSpace(request.Model.ShopName)
      ? null
      : request.Model.ShopName.Trim();
    if (!string.IsNullOrWhiteSpace(shopName))
    {
      specBuilder.Add<GetFeedbacksByShopNamesSpecification>(shopName);
    }

    var ratings = request.Model.Ratings?
      .Where(r => r is >= 1 and <= 5)
      .Distinct()
      .ToList();
    if (ratings?.Any() == true)
    {
      specBuilder.Add<GetFeedbacksByRatingsSpecification>(ratings);
    }

    var statuses = request.Model.Statuses?
      .Where(s => Enum.IsDefined(typeof(FeedbackStatusEnum), s))
      .Distinct()
      .ToList();
    if (statuses?.Any() == true)
    {
      specBuilder.Add<GetFeedbacksByStatusesSpecification>(statuses);
    }

    var visibilityTypes = request.Model.IsPublic?
      .Distinct()
      .ToList();
    if (visibilityTypes?.Any() == true)
    {
      specBuilder.Add<GetFeedbacksByPublicitySpecification>(visibilityTypes);
    }

    var (startDate, endDate) = ResolveDateRange(request.Model);
    if (startDate.HasValue && endDate.HasValue)
    {
      specBuilder.Add<GetFeedbacksByDateRangeSpecification>(startDate.Value, endDate.Value);
    }
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

  private static (DateTime? StartDate, DateTime? EndDate) ResolveDateRange(GetFilteredFeedbacksModel model)
  {
    if (model.StartDate.HasValue && model.EndDate.HasValue)
    {
      var start = DateTime.SpecifyKind(model.StartDate.Value, DateTimeKind.Utc);
      var end = DateTime.SpecifyKind(model.EndDate.Value, DateTimeKind.Utc);
      return (start, end);
    }

    if (model.DateRange == DateRangeFilterEnum.All)
    {
      return (null, null);
    }

    var todayUtc = DateTime.UtcNow.Date;
    var startDate = model.DateRange switch
    {
      DateRangeFilterEnum.Today => todayUtc,
      DateRangeFilterEnum.Last7Days => todayUtc.AddDays(-6),
      DateRangeFilterEnum.LastMonth => todayUtc.AddDays(-29),
      _ => (DateTime?)null
    };

    return startDate.HasValue ? (startDate, DateTime.UtcNow) : (null, null);
  }
}
