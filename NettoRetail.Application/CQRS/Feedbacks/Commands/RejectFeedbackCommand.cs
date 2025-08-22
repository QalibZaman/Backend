using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Feedbacks.Commands;

public record RejectFeedbackCommand(Guid Id, string Reason) : IRequest;

public class RejectFeedbackCommandHandler(
    IFeedbackRepository feedbackRepository//,
    //IEmailSender emailSender,
    //IEmailBuilderService emailBuilderService
    )
    : IRequestHandler<RejectFeedbackCommand>
{
    public async Task Handle(RejectFeedbackCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetFeedbackByIdSpecification(request.Id);
        specification.AddInclude(f => f.Account);
        var feedback = await feedbackRepository.GetAsync(specification, cancellationToken)
                      ?? throw new NotFoundException("Review not found or cannot be rejected.");

        feedback.Status = FeedbackStatusEnum.Rejected;
        feedback.RejectionReason = request.Reason;
        await feedbackRepository.UpdateAsync(feedback, cancellationToken);

        //SmtpClient don`t working
        //var emailModel = await emailBuilderService.BuildFeedbackRejectedEmailAsync(feedback.Account.Email, feedback.Account.FirstName, request.Reason);
        //await emailSender.SendAsync(emailModel);
    }
}