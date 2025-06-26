using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Feedback;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Feedbacks.Commands;

public record CreateFeedbackCommand(CreateFeedbackModel Model, Guid UserId) : IRequest<FeedbackResponse>;
public class CreateFeedbackCommandHandler(
  IFeedbackRepository feedbackRepository,
  IAccountRepository accountRepository,
  IUnitOfWork unitOfWork) : IRequestHandler<CreateFeedbackCommand, FeedbackResponse>
{
  public async Task<FeedbackResponse> Handle(CreateFeedbackCommand request, CancellationToken cancellationToken)
  {
    var accountByIdSpecification = new GetAccountByIdSpecification(request.UserId);
    var account = await accountRepository.GetAsync(accountByIdSpecification, cancellationToken) ?? throw new MyUnauthorizedException("Account no found.");

    var feedback = request.Model.Adapt<FeedbackEntity>();

    feedback.AccountId = account.Id;
    feedback.DateSubmitted = DateTime.UtcNow;
    feedback.Status = FeedbackStatusEnum.Pending;
    if (account.Shop != null) { feedback.ShopName = account.Shop.Name; }
    if (request.Model.IsPublic) { feedback.StaffName = account.FullName; }
    feedback.Role = account.Role;

    using var transaction = unitOfWork.BeginTransaction();

    try
    {
      await feedbackRepository.AddAsync(feedback, cancellationToken);

      transaction.Commit();
    }
    catch (Exception)
    {
      transaction.Rollback();
      throw;
    }

    return feedback.Adapt<FeedbackResponse>();
  }
}
