using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Feedback;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Feedbacks.Commands;

public record CreateFeedbackCommand(CreateFeedbackModel Model, Guid UserId) : IRequest<FeedbackResponse>;
public class CreateFeedbackCommandHandler(
  IFeedbackRepository feedbackRepository,
  IStaffRepository accountRepository,
  IMapper mapper) : IRequestHandler<CreateFeedbackCommand, FeedbackResponse>
{
  public async Task<FeedbackResponse> Handle(CreateFeedbackCommand request, CancellationToken cancellationToken)
  {
    var accountByIdSpecification = new GetStaffByIdSpecification(request.UserId);
    accountByIdSpecification.AddInclude(x => x.Shop!);
    var account = await accountRepository.GetAsync(accountByIdSpecification, cancellationToken) ?? throw new NotFoundException("Account no found.");

    var feedback = mapper.Map<FeedbackEntity>(request.Model);

    feedback.AccountId = account.Id;
    feedback.DateSubmitted = DateTime.UtcNow;
    feedback.Status = FeedbackStatusEnum.Pending;
    if (account.Shop != null) { feedback.ShopName = account.Shop.Name; }
    if (request.Model.IsPublic) { feedback.StaffName = account.FullName; }
    else { feedback.StaffName = FeedbackConstants.AnonymousStaffName; }

    await feedbackRepository.AddAsync(feedback, cancellationToken);

    return mapper.Map<FeedbackResponse>(feedback);
  }
}
