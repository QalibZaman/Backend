using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.PasswordHistory;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.Services.IPasswordService;
public class PasswordService(
       IStaffRepository accountRepository,
       ICustomerRepository customerRepository,
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

    public async Task ChangeCustomerPasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var customer = await GetCustomerUserAsync(userId, cancellationToken);

        await ValidatePasswordsAsync(customer, currentPassword, newPassword, cancellationToken);

        await ChangeAndCommitAsync(customer, newPassword, cancellationToken);

        await SendConfirmationEmailAsync(customer.Email, customer.FirstName, userId);
    }

    private async Task<StaffEntity> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var spec = new GetStaffByIdSpecification(userId);
        return await accountRepository.GetAsync(spec, cancellationToken)
               ?? throw new NotFoundException("User not found.");
    }

    private async Task<CustomerEntity> GetCustomerUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var spec = new GetCustomerByIdSpecification(userId);
        return await customerRepository.GetAsync(spec, cancellationToken)
               ?? throw new NotFoundException("User not found.");
    }

    private async Task ValidatePasswordsAsync(BaseUserEntity user, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        if (!PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
            throw new BadRequestException(ValidationErrorMessages.Password.CurrentPasswordIncorrectErrorMessage);

        if (PasswordHasher.VerifyPassword(newPassword, user.PasswordHash))
            throw new BadRequestException(ValidationErrorMessages.Password.NewPasswordMatchesCurrentErrorMessage);

        var spec = new GetPasswordHistoryByAccountIdSpecification(user.Id);
        var histories = await passwordHistoryRepository.GetAllBySpecAsync(spec, cancellationToken);

        if (histories.Any(h => PasswordHasher.VerifyPassword(newPassword, h.HashedPassword)))
            throw new BadRequestException("New password cannot match any previously used passwords.");
    }

    private async Task ChangeAndCommitAsync(BaseUserEntity user, string newPassword, CancellationToken cancellationToken)
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
            user.PasswordUpdatedAt = DateTime.UtcNow;

            if (user is StaffEntity staffUser)
            {
                await accountRepository.UpdateAsync(staffUser, cancellationToken);
            }
            else if (user is CustomerEntity customerUser)
            {
                await customerRepository.UpdateAsync(customerUser, cancellationToken);
            }

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

    public async Task<bool> ValidatePasswordAsync(Guid userId, string password, CancellationToken cancellationToken)
    {
       var customer = await GetCustomerUserAsync(userId, cancellationToken);
       return PasswordHasher.VerifyPassword(password, customer.PasswordHash);
    }
}
