using MediatR;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Feedbacks.Commands;

public record BulkApproveFeedbacksCommand(List<Guid> Ids) : IRequest;

public class BulkApproveFeedbacksCommandHadler(IFeedbackRepository feedabackRepository)
    : IRequestHandler<BulkApproveFeedbacksCommand>
{
    public async Task Handle(BulkApproveFeedbacksCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids.IsNullOrEmpty()) throw new BadRequestException("At least one Id must be provided.");
        var specification = new GetBulkExistedFeedbacksSpecification(request.Ids);
        var feedbacks = await feedabackRepository.GetAllBySpecAsync(specification, cancellationToken);
        if (feedbacks == null || !feedbacks.Any()) throw new NotFoundException("No valid feedbacks found.");

        foreach (var feedback in feedbacks)
        {
            if (feedback.Status != FeedbackStatusEnum.Pending)
                throw new BadRequestException("Only pending feedbacks can be approved.");
            feedback.Status = FeedbackStatusEnum.Approved;
        }
        await feedabackRepository.UpdateManyAsync(feedbacks, cancellationToken);
    }
}
