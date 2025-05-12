using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.PasswordHistory;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Password;

public record ChangePasswordCommand(Guid UserId, ChangePasswordRequest Model) : IRequest;

public class ChangePasswordCommandHandler(
    IAccountRepository accountRepository,
    IPasswordHistoryRepository passwordHistoryRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetAccountByIdSpecification(request.UserId);

        var user = await accountRepository
            .GetAsync(specification, cancellationToken) ??
            throw new NotFoundException("User not found.");

        if (!PasswordHasher.VerifyPassword(request.Model.CurrentPassword, user.PasswordHash))
        {
            throw new BadRequestException("Password is not correct");
        }

        if (PasswordHasher.VerifyPassword(request.Model.NewPassword, user.PasswordHash))
        {
            throw new BadRequestException("New password cannot be the same as the current password");
        }

        var passwordHistorySpec = new GetPasswordHistoryByAccountIdSpecification(user.Id);
        var passwordHistories = await passwordHistoryRepository.GetAllBySpecAsync(passwordHistorySpec, cancellationToken);

        var isPasswordUsedBefore = passwordHistories
            .Any(ph => PasswordHasher.VerifyPassword(request.Model.NewPassword, ph.HashedPassword));

        if (isPasswordUsedBefore)
        {
            throw new BadRequestException("New password cannot match any previously used passwords.");
        }

        // Begin transaction to ensure password history and password update are atomic
        var transaction = unitOfWork.BeginTransaction();
        try
        {
            await passwordHistoryRepository.AddAsync(
                new PasswordHistoryEntity
                {
                    AccountId = user.Id,
                    HashedPassword = user.PasswordHash,
                    CreatedAt = DateTime.UtcNow
                },
                cancellationToken);

            user.PasswordHash = PasswordHasher.HashPassword(request.Model.NewPassword);

            await accountRepository.UpdateAsync(user, cancellationToken);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        var emailModel = await emailBuilderService.BuildPasswordChangeConfirmationEmailAsync(user.Email, user.FirstName);
        await emailSender.SendAsync(emailModel);
    }
}
