using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Review;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Reviews.Queries;

public record GetReviewDetailQuery(Guid ReviewId) : IRequest<ReviewDetailResponse>;

public class GetReviewDetailQueryHandler(IReviewRepository reviewRepository)
    : IRequestHandler<GetReviewDetailQuery, ReviewDetailResponse>
{
    public async Task<ReviewDetailResponse> Handle(GetReviewDetailQuery request, CancellationToken cancellationToken)
    {
        var spec = new ReviewDetailSpecification(request.ReviewId);
        var review = await reviewRepository.GetAsync(spec, cancellationToken)
                      ?? throw new NotFoundException("Review not found.");

        var reviewDetail = new ReviewDetailResponse
        {
            Id = review.Id,
            ShopName = review.ShopName,
            StaffName = review.StaffName,
            Role = review.Role,
            Rating = review.Rating,
            ReviewText = review.ReviewText,
            RejectionReason = review.RejectionReason,
            DateSubmitted = review.DateSubmitted,
            Status = review.Status
        };

        return reviewDetail;

    }
}