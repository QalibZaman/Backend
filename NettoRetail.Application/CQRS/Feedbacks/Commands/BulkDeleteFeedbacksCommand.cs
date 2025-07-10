using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Reviews.Commands;

public record BulkDeleteFeedbacksCommand(List<Guid> FeedbacksIds) : IRequest;

public class BulkDeleteFeedbacksCommandHandler(IFeedbackRepository feedbackRepository)
    : IRequestHandler<BulkDeleteFeedbacksCommand>
{
  public async Task Handle(BulkDeleteFeedbacksCommand request, CancellationToken cancellationToken)
  {
    var specification = new GetBulkExistedFeedbacksSpecification(request.FeedbacksIds);
    var feedbacks = await feedbackRepository.GetAllBySpecAsync(specification, cancellationToken);
    if (feedbacks == null || !feedbacks.Any()) throw new NotFoundException("No valid feedbacks found.");
    await feedbackRepository.DeleteManyBySpecAsync(specification, cancellationToken);
  }
}
