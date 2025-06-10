using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Reviews;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Reviews.Commands;

public record BulkRejectReviewsCommand(List<Guid> ReviewIds, string Reason) : IRequest;

public class BulkRejectReviewsCommandHandler(IReviewRepository reviewRepository)
    : IRequestHandler<BulkRejectReviewsCommand>
{
    public async Task Handle(BulkRejectReviewsCommand request, CancellationToken cancellationToken)
    {
        var spec = new BulkRejectedReviewSpecification(request.ReviewIds);
        var reviews = await reviewRepository.GetAllBySpecAsync(spec, cancellationToken);

        if (!reviews.Any()) throw new NotFoundException("No valid reviews found.");

        foreach (var review in reviews)
        {
            review.Status = ReviewStatusEnum.Rejected;
            review.RejectionReason = request.Reason;
        }

        await reviewRepository.UpdateManyAsync(reviews, cancellationToken);
    }
}