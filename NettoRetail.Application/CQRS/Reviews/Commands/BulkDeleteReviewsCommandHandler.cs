using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Reviews;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Reviews.Commands;

public record BulkDeleteReviewsCommand(List<Guid> ReviewIds) : IRequest;

public class BulkDeleteReviewsCommandHandler(IReviewRepository reviewRepository)
    : IRequestHandler<BulkDeleteReviewsCommand>
{
    public async Task Handle(BulkDeleteReviewsCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetBulkExistedReviewSpecification(request.ReviewIds);
        var reviews = await reviewRepository.GetAllBySpecAsync(spec, cancellationToken) ?? throw new NotFoundException("No valid reviews found.");

        var deleteSpec = new GetBulkExistedReviewSpecification(reviews.Select(r => r.Id).ToList());
        await reviewRepository.DeleteManyBySpecAsync(spec, cancellationToken);
    }
}