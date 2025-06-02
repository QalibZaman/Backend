using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Reviews;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Reviews.Commands;

public record ApproveReviewCommand(Guid ReviewId) : IRequest;
public class ApproveReviewCommandHandler(IReviewRepository reviewRepository)
    : IRequestHandler<ApproveReviewCommand>
{
    public async Task Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        var spec = new PendingReviewSpecification(request.ReviewId);
        var review = await reviewRepository.GetAsync(spec, cancellationToken)
                      ?? throw new NotFoundException("Pending review not found.");

        if (review.Status != ReviewStatusEnum.Pending)
            throw new BadRequestException("Only pending reviews can be approved.");

        review.Status = ReviewStatusEnum.Approved;
        await reviewRepository.UpdateAsync(review, cancellationToken);
    }
}