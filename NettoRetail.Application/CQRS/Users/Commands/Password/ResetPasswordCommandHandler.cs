using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications.PasswordHistory;
using NettoRetail.Shared.Utilities;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using NettoRetail.DataAccess.Identity;

namespace NettoRetail.Application.CQRS.Users.Commands.Password
{
    public record ResetPasswordCommand(ResetPasswordRequest Model) : IRequest;

    public class ResetPasswordCommandHandler(
        IStaffRepository accountRepository,
        IPasswordHistoryRepository passwordHistoryRepository,
        ISessionTerminationService sessionTerminationService,
        IUnitOfWork unitOfWork,
        IJwtManager jwtManager,
        UserManager<ApplicationUser> userManager)
        : IRequestHandler<ResetPasswordCommand>
    {
        public async Task Handle(
            ResetPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var claimsPrincipal = jwtManager.ValidateResetToken(request.Model.Token);

            var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value
                        ?? throw new BadRequestException("Invalid token payload.");

            var identityUser = await userManager.FindByEmailAsync(email)
                               ?? throw new NotFoundException("User not found.");

            var specification = new GetStaffByEmailSpecification(email);

            var user = await accountRepository.GetAsync(
                           specification,
                           cancellationToken)
                       ?? throw new NotFoundException("User not found.");

            var passwordHasher = userManager.PasswordHasher;

            // Check if new password is the same as current password
            var currentPasswordResult = passwordHasher.VerifyHashedPassword(
                identityUser,
                identityUser.PasswordHash!,
                request.Model.NewPassword);

            if (currentPasswordResult == PasswordVerificationResult.Success)
            {
                throw new BadRequestException(
                    "New password cannot be the same as the current password.");
            }

            // Get password history
            var passwordHistorySpec =
                new GetPasswordHistoryByAccountIdSpecification(user.Id);

            var passwordHistories =
                await passwordHistoryRepository.GetAllBySpecAsync(
                    passwordHistorySpec,
                    cancellationToken);

            // Check password history
            var isPasswordUsedBefore = passwordHistories.Any(ph => passwordHasher.VerifyHashedPassword(
                                                                       identityUser,
                                                                       ph.HashedPassword,
                                                                       request.Model.NewPassword)
                                                                   == PasswordVerificationResult.Success);

            if (isPasswordUsedBefore)
            {
                throw new BadRequestException(
                    "New password cannot match any previously used passwords.");
            }

            var transaction = unitOfWork.BeginTransaction();

            try
            {
                // Store current password in history
                await passwordHistoryRepository.AddAsync(
                    new PasswordHistoryEntity
                    {
                        AccountId = user.Id,
                        HashedPassword = identityUser.PasswordHash!,
                        CreatedAt = DateTime.UtcNow
                    },
                    cancellationToken);

                // Hash the new password using Identity's hasher
                identityUser.PasswordHash = passwordHasher.HashPassword(
                    identityUser,
                    request.Model.NewPassword);

                // Update Identity user
                var updateResult = await userManager.UpdateAsync(identityUser);

                if (!updateResult.Succeeded)
                {
                    throw new BadRequestException(
                        string.Join(
                            ", ",
                            updateResult.Errors.Select(e => e.Description)));
                }

                // Update StaffEntity
                user.PasswordUpdatedAt = DateTime.UtcNow;

                if (user.Status == StaffStatusEnum.CreatePassword)
                {
                    user.Status = StaffStatusEnum.Active;
                }

                await accountRepository.UpdateAsync(
                    user,
                    cancellationToken);

                await sessionTerminationService.TerminateSessionsAsync(
                    user.Id,
                    cancellationToken);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}