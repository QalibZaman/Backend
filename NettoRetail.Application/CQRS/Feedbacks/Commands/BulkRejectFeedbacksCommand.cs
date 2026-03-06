using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Feedbacks.Commands;

public record BulkRejectFeedbacksCommand(List<Guid> Ids, string Reason) : IRequest;

public class BulkRejectFeedbacksCommandHandler(IFeedbackRepository feedbackRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService)
    : IRequestHandler<BulkRejectFeedbacksCommand>
{
    public async Task Handle(BulkRejectFeedbacksCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids is null || request.Ids.Count == 0)
            throw new BadRequestException("At least one Id must be provided.");

        var reason = FeedbackRejectionReason.NormalizeOrThrow(request.Reason);
        var specification = new GetBulkExistedFeedbacksSpecification(request.Ids);
        specification.AddInclude(f => f.Account);
        var feedbacks = await feedbackRepository.GetAllBySpecAsync(specification, cancellationToken);
        if (feedbacks == null || !feedbacks.Any()) throw new NotFoundException("No valid feedbacks found.");

        var emailAddresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var feedback in feedbacks)
        {
            if (feedback.Status != FeedbackStatusEnum.Pending)
                throw new BadRequestException("Only pending feedbacks can be rejected.");
            feedback.Status = FeedbackStatusEnum.Rejected;
            feedback.RejectionReason = reason;
            feedback.IsVisible = false;

            //SmtpClient not  working
            var email = feedback.Account?.Email;
            if (!string.IsNullOrWhiteSpace(email))
                emailAddresses.Add(email);
        }

        await feedbackRepository.UpdateManyAsync(feedbacks, cancellationToken);

        if (emailAddresses.Count == 0)
            return;

        var bulkEmailModel = await emailBuilderService.BuildFeedbackRejectedEmailsAsync(emailAddresses.ToList(), reason);
        await emailSender.SendBulkAsync(bulkEmailModel);
    }
}
