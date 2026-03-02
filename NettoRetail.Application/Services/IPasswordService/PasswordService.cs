using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.PasswordHistory;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.Services.IPasswordService;
public class PasswordService(
       IStaffRepository accountRepository,
       IPasswordHistoryRepository passwordHistoryRepository,
       ISessionTerminationService sessionTerminationService,
       IEmailSender emailSender,
       IEmailBuilderService emailBuilderService,
       IUnitOfWork unitOfWork,
       ILogger<PasswordService> logger)
       : IPasswordService
{
    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(userId, cancellationToken);

        await ValidatePasswordsAsync(user, currentPassword, newPassword, cancellationToken);

        await ChangeAndCommitAsync(user, newPassword, cancellationToken);

        await SendConfirmationEmailAsync(user.Email, user.FirstName, userId);
    }

    private async Task<StaffEntity> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var spec = new GetStaffByIdSpecification(userId);
        return await accountRepository.GetAsync(spec, cancellationToken)
               ?? throw new NotFoundException("User not found.");
    }

    private async Task ValidatePasswordsAsync(StaffEntity user, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        if (!PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
            throw new BadRequestException("Current password is incorrect.");

        if (PasswordHasher.VerifyPassword(newPassword, user.PasswordHash))
            throw new BadRequestException("New password cannot be the same as the current one.");

        var spec = new GetPasswordHistoryByAccountIdSpecification(user.Id);
        var histories = await passwordHistoryRepository.GetAllBySpecAsync(spec, cancellationToken);

        if (histories.Any(h => PasswordHasher.VerifyPassword(newPassword, h.HashedPassword)))
            throw new BadRequestException("New password cannot match any previously used passwords.");
    }

    private async Task ChangeAndCommitAsync(StaffEntity user, string newPassword, CancellationToken cancellationToken)
    {
        var transaction = unitOfWork.BeginTransaction();
        try
        {
            await passwordHistoryRepository.AddAsync(
                new PasswordHistoryEntity
                {
                    AccountId = user.Id,
                    HashedPassword = user.PasswordHash,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            await accountRepository.UpdateAsync(user, cancellationToken);
            await sessionTerminationService.TerminateSessionsAsync(user.Id, cancellationToken);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private async Task SendConfirmationEmailAsync(string email, string firstName, Guid userId)
    {
        try
        {
            var model = await emailBuilderService.BuildPasswordChangeConfirmationEmailAsync(email, firstName);
            await emailSender.SendAsync(model, userId);
            logger.LogInformation("Password change confirmation email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Password changed successfully, but email failed to send to {Email}", email);
        }
    }
}
