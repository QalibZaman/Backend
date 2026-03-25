using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Reviews;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Reviews.Queries;

public record GetReviewDetailQuery(Guid ReviewId) : IRequest<ReviewDetailResponse>;

public class GetReviewDetailQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    : IRequestHandler<GetReviewDetailQuery, ReviewDetailResponse>
{
    public async Task<ReviewDetailResponse> Handle(GetReviewDetailQuery request, CancellationToken cancellationToken)
    {
        var spec = new ReviewDetailSpecification(request.ReviewId);
        var review = await reviewRepository.GetAsync(spec, cancellationToken)
                      ?? throw new NotFoundException("Review not found.");

        var reviewDetail = mapper.Map<ReviewDetailResponse>(review);

        return reviewDetail;

    }
}
