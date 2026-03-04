using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Feedbacks.Commands;

public record BulkDeleteFeedbacksCommand(List<Guid> FeedbacksIds) : IRequest;

public class BulkDeleteFeedbacksCommandHandler(IFeedbackRepository feedbackRepository)
    : IRequestHandler<BulkDeleteFeedbacksCommand>
{
  public async Task Handle(BulkDeleteFeedbacksCommand request, CancellationToken cancellationToken)
  {
    if (request.FeedbacksIds is null || request.FeedbacksIds.Count == 0)
      throw new BadRequestException("At least one Id must be provided.");
    var specification = new GetBulkExistedFeedbacksSpecification(request.FeedbacksIds);
    var feedbacks = await feedbackRepository.GetAllBySpecAsync(specification, cancellationToken);
    if (feedbacks == null || !feedbacks.Any()) throw new NotFoundException("No valid feedbacks found.");
    await feedbackRepository.DeleteManyBySpecAsync(specification, cancellationToken);
  }
}
