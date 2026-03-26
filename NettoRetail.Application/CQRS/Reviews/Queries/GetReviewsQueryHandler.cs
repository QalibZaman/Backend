using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Request.Review;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Reviews.Queries;

public record GetReviewsQuery(string? ShopName, int? Rating, ReviewStatusEnum? Status, DateRangeFilterEnum DateRange) : IRequest<List<ReviewResponse>>;

public class GetReviewsQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    : IRequestHandler<GetReviewsQuery, List<ReviewResponse>>
{
    public async Task<List<ReviewResponse>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await reviewRepository.GetFilteredReviewsAsync(request.ShopName, request.Rating, request.Status, request.DateRange, cancellationToken);
        return mapper.Map<List<ReviewResponse>>(reviews);
    }
}
