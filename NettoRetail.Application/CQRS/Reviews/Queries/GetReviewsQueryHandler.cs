using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request.Review;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Reviews.Queries;

public record GetReviewsQuery(string? ShopName, int? Rating, ReviewStatusEnum? Status, DateRangeFilterEnum DateRange) : IRequest<List<ReviewResponse>>;
public class GetReviewsQueryHandler(IReviewRepository reviewRepository)
    : IRequestHandler<GetReviewsQuery, List<ReviewResponse>>
{
    public async Task<List<ReviewResponse>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
    {
        var filter = request;
        var reviews = await reviewRepository.GetAllAsync(cancellationToken);
        var query = reviews.AsQueryable();

        if (!string.IsNullOrEmpty(filter.ShopName))
            query = query.Where(r => r.ShopName.Contains(filter.ShopName));

        if (filter.Rating.HasValue)
            query = query.Where(r => r.Rating == filter.Rating.Value);

        if (filter.Status.HasValue)
            query = query.Where(r => r.Status == filter.Status.Value);

        if (filter.DateRange == DateRangeFilterEnum.Today)
            query = query.Where(r => r.DateSubmitted.Date == DateTime.UtcNow.Date);
        else if (filter.DateRange == DateRangeFilterEnum.Last7Days)
            query = query.Where(r => r.DateSubmitted >= DateTime.UtcNow.AddDays(-7));
        else if (filter.DateRange == DateRangeFilterEnum.LastMonth)
            query = query.Where(r => r.DateSubmitted >= DateTime.UtcNow.AddMonths(-1));

        return query.Select(r => r.Adapt<ReviewResponse>()).ToList();
    }
}
