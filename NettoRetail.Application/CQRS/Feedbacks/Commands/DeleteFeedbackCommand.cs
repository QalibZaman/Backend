using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Feedbacks.Commands;

public record DeleteFeedbackCommand(Guid Id) : IRequest;

public class DeleteFeedbackCommandHandler(IFeedbackRepository feedbackRepository) : IRequestHandler<DeleteFeedbackCommand>
{
  public async Task Handle(DeleteFeedbackCommand request, CancellationToken cancellationToken)
  {
    var specification = new GetFeedbackByIdSpecification(request.Id);

    var feedback = await feedbackRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException($"There is no feedback with Id: {request.Id}");
    await feedbackRepository.DeleteAsync(feedback, cancellationToken);
  }
}
