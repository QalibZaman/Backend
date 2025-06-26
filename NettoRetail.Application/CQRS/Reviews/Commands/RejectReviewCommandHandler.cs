using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Reviews;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Reviews.Commands;

public record RejectReviewCommand(Guid ReviewId, string Reason) : IRequest;

public class RejectReviewCommandHandler(IReviewRepository reviewRepository)
    : IRequestHandler<RejectReviewCommand>
{
    public async Task Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        var spec = new RejectedReviewSpecification(request.ReviewId);
        var review = await reviewRepository.GetAsync(spec, cancellationToken)
                      ?? throw new NotFoundException("Review not found or cannot be rejected.");

        review.Status = ReviewStatusEnum.Rejected;
        review.RejectionReason = request.Reason;
        await reviewRepository.UpdateAsync(review, cancellationToken);
    }
}
