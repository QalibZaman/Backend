using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Feedbacks.Queries;

public record GetFeedbackDetailQuery(Guid FeedbackId) : IRequest<FeedbackDetailResponse>;

public class GetFeedbackDetailQueryHandler(IFeedbackRepository feedbackRepository, IMapper mapper)
    : IRequestHandler<GetFeedbackDetailQuery, FeedbackDetailResponse>
{
    public async Task<FeedbackDetailResponse> Handle(GetFeedbackDetailQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetFeedbackByIdSpecification(request.FeedbackId);
        spec.AddInclude(f => f.Account);
        var feedback = await feedbackRepository.GetAsync(spec, cancellationToken)
                      ?? throw new NotFoundException("Feedback not found.");

        var feedbackDetail = mapper.Map<FeedbackDetailResponse>(feedback);

        feedbackDetail.Email = feedback.Account?.Email;
        return feedbackDetail;

    }
}
