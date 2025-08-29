using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Feedbacks.Commands;

public record BulkRejectFeedbacksCommand(List<Guid> Ids, string Reason) : IRequest;

public class BulkRejectFeedbacksCommandHadler(IFeedbackRepository feedbackRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService)
    : IRequestHandler<BulkRejectFeedbacksCommand>
{
    public async Task Handle(BulkRejectFeedbacksCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetBulkExistedFeedbacksSpecification(request.Ids);
        var feedbacks = await feedbackRepository.GetAllBySpecAsync(specification, cancellationToken);
        if (feedbacks == null || !feedbacks.Any()) throw new NotFoundException("No valid feedbacks found.");

        foreach (var feedback in feedbacks)
        {
            if (feedback.Status != FeedbackStatusEnum.Pending)
                throw new BadRequestException("Only pending feedbacks can be approved.");
            feedback.Status = FeedbackStatusEnum.Rejected;
            feedback.RejectionReason = request.Reason;
            //SmtpClient not  working

            var emailModel = await emailBuilderService.BuildFeedbackRejectedEmailAsync(feedback.Account.Email, feedback.Account.FirstName, request.Reason);
            await emailSender.SendAsync(emailModel);
        }

        await feedbackRepository.UpdateManyAsync(feedbacks, cancellationToken);
    }
}
