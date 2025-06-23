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
    var model = request.Model;
    var accountByIdSpecification = new GetAccountByIdSpecification(request.UserId);
    var account = await accountRepository.GetAsync(accountByIdSpecification, cancellationToken) ?? throw new MyUnauthorizedException("Account no found.");
    var shop = account.Shop;
    var role = account.Role;

    var feedback = new FeedbackEntity
    {
      Rating = model.Rating,
      Title = model.Title,
      Text = model.Text,
      IsPublic = model.IsPublic,
      AccountId = account.Id,
      DateSubmitted = DateTime.UtcNow,
      Status = FeedbackStatusEnum.Pending
    };

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

    return new FeedbackResponse
    {
      Id = feedback.Id,
      Rating = feedback.Rating,
      Title = feedback.Title,
      Text = feedback.Text,
      IsPublic = feedback.IsPublic,
      StaffName = account.FullName,
      Role = role,
      DateSubmitted = feedback.DateSubmitted,
      Status = feedback.Status.ToString(),
      ShopName = shop?.Name
    };
  }
}
