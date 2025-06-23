using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Feedbacks.Queries;
public record GetFeedbackByIdQuery(Guid Id) : IRequest<FeedbackResponse>;
public class GetFeedbackByIdQueryHandler(IFeedbackRepository feedbackRepository)
    : IRequestHandler<GetFeedbackByIdQuery, FeedbackResponse>
{
  public async Task<FeedbackResponse> Handle(GetFeedbackByIdQuery request, CancellationToken cancellationToken)
  {
    var specification = new GetFeedbackByIdSpecification(request.Id);
    var feedback = await feedbackRepository.GetAsync(specification, cancellationToken)
                   ?? throw new NotFoundException("Feedback not found.");

    return new FeedbackResponse
    {
      Id = feedback.Id,
      Rating = feedback.Rating,
      Title = feedback.Title,
      Text = feedback.Text,
      IsPublic = feedback.IsPublic,
      StaffName = feedback.Account?.FullName ?? "Anonymous",
      Role = feedback.Account?.Role,
      DateSubmitted = feedback.DateSubmitted,
      Status = feedback.Status.ToString(),
      ShopName = feedback.Account?.Shop?.Name
    };
  }
}