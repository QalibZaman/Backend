using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Reviews;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Reviews.Commands;

public record BulkApproveReviewsCommand(List<Guid> ReviewIds) : IRequest;

public class BulkApproveReviewsHandler(IReviewRepository reviewRepository) : IRequestHandler<BulkApproveReviewsCommand>
{
    public async Task Handle(BulkApproveReviewsCommand request, CancellationToken cancellationToken)
    {
        var spec = new BulkApproveReviewsSpecification(request.ReviewIds);
        var reviews = await reviewRepository.GetAllBySpecAsync(spec, cancellationToken) ?? throw new NotFoundException("No valid reviews found.");

        foreach (var review in reviews)
        {
            review.Status = ReviewStatusEnum.Approved;
        }

        await reviewRepository.UpdateManyAsync(reviews, cancellationToken);
    }
}