using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Reviews;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Reviews.Commands;

public record DeleteReviewCommand(Guid ReviewId) : IRequest;

public class DeleteReviewCommandHandler(IReviewRepository reviewRepository)
    : IRequestHandler<DeleteReviewCommand>
{
    public async Task Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetExistedReviewSpecification(request.ReviewId);
        var review = await reviewRepository.GetAsync(spec, cancellationToken)
                      ?? throw new NotFoundException("Review not found.");

        await reviewRepository.DeleteAsync(review, cancellationToken);
    }
}
