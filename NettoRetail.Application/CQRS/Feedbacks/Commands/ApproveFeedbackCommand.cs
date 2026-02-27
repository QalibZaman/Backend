using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Feedbacks.Commands;

public record ApproveFeedbackCommand(Guid Id) : IRequest;
public class ApproveFeedbackCommandHandler(IFeedbackRepository feedbackRepository)
    : IRequestHandler<ApproveFeedbackCommand>
{
    public async Task Handle(ApproveFeedbackCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetFeedbackByIdSpecification(request.Id);
        var feedback = await feedbackRepository.GetAsync(specification, cancellationToken)
                      ?? throw new NotFoundException("Feedback not found.");

        if (feedback.Status != FeedbackStatusEnum.Pending)
            throw new BadRequestException("Only pending feedbacks can be approved.");

        feedback.Status = FeedbackStatusEnum.Approved;
        feedback.IsVisible = false;
        await feedbackRepository.UpdateAsync(feedback, cancellationToken);
    }
}
