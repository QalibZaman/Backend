using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Feedback.Queries;

public record GetFeedbackDetailQuery(Guid FeedbackId) : IRequest<FeedbackDetailResponse>;

public class GetFeedbackDetailQueryHandler(IFeedbackRepository feedbackRepository)
    : IRequestHandler<GetFeedbackDetailQuery, FeedbackDetailResponse>
{
    public async Task<FeedbackDetailResponse> Handle(GetFeedbackDetailQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetFeedbackByIdSpecification(request.FeedbackId);
        var feedback = await feedbackRepository.GetAsync(spec, cancellationToken)
                      ?? throw new NotFoundException("Feedback not found.");

        var feedbackDetail = feedback.Adapt<FeedbackDetailResponse>();

        feedbackDetail.Email = feedback.Account.Email;
        return feedbackDetail;

    }
}