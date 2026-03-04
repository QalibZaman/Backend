using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Feedbacks.Commands;

public record ToggleVisibilityCommand(Guid Id, bool IsVisible) : IRequest;
public class ToggleVisibilityCommandHandler(IFeedbackRepository feedbackRepository)
    : IRequestHandler<ToggleVisibilityCommand>
{
    public async Task Handle(ToggleVisibilityCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetFeedbackByIdSpecification(request.Id);
        var feedback = await feedbackRepository.GetAsync(specification, cancellationToken)
                      ?? throw new NotFoundException("Feedback not found.");

        if (feedback.Status != FeedbackStatusEnum.Approved || !feedback.IsPublic)
            throw new BadRequestException("Not available for anonymous or unapproved feedbacks.");

        feedback.IsVisible = request.IsVisible;
        await feedbackRepository.UpdateAsync(feedback, cancellationToken);
    }
}
