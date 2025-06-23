using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Feedbacks.Commands;

public record UpdateFeedbackStatusCommand(Guid FeedbackId, FeedbackStatusEnum NewStatus)
    : IRequest<FeedbackResponse>;

public class UpdateFeedbackStatusCommandHandler(
    IFeedbackRepository feedbackRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateFeedbackStatusCommand, FeedbackResponse>
{
  public async Task<FeedbackResponse> Handle(UpdateFeedbackStatusCommand request, CancellationToken cancellationToken)
  {
    var spec = new GetFeedbackByIdSpecification(request.FeedbackId);

    var feedback = await feedbackRepository.GetAsync(spec, cancellationToken)
        ?? throw new NotFoundException("Feedback not found");

    var transaction = unitOfWork.BeginTransaction();
    try
    {
      feedback.Status = request.NewStatus;
      await feedbackRepository.UpdateAsync(feedback, cancellationToken);
      transaction.Commit();
    }
    catch
    {
      transaction.Rollback();
      throw;
    }

    var response = feedback.Adapt<FeedbackResponse>();
    return response;
  }
}
