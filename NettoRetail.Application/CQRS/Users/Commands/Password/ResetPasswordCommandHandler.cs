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

namespace NettoRetail.Application.CQRS.Users.Commands.Password
{
    public record ResetPasswordCommand(ResetPasswordRequest Model) : IRequest;
    public class ResetPasswordCommandHandler(
        IStaffRepository accountRepository,
        IPasswordHistoryRepository passwordHistoryRepository,
        ISessionTerminationService sessionTerminationService,
        IUnitOfWork unitOfWork,
        IJwtManager jwtManager)
        : IRequestHandler<ResetPasswordCommand>
    {
        public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var claimsPrincipal = jwtManager.ValidateResetToken(request.Model.Token);

            var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value ??
                throw new BadRequestException("Invalid token payload.");

            var specification = new GetStaffByEmailSpecification(email);
            var user = await accountRepository.GetAsync(specification, cancellationToken) ??
                throw new NotFoundException("User not found.");

            var isNewPasswordSameAsCurrent = PasswordHasher.VerifyPassword(
                request.Model.NewPassword,
                user.PasswordHash);

            if (isNewPasswordSameAsCurrent)
            {
                throw new BadRequestException(
                    "New password cannot be the same as the current password");
            }

            var passwordHistorySpec = new GetPasswordHistoryByAccountIdSpecification(user.Id);
            var passwordHistories = await passwordHistoryRepository.GetAllBySpecAsync(passwordHistorySpec, cancellationToken);

            var isPasswordUsedBefore = passwordHistories
                .Any(ph => PasswordHasher.VerifyPassword(request.Model.NewPassword, ph.HashedPassword));

            if (isPasswordUsedBefore)
            {
                throw new BadRequestException("New password cannot match any previously used passwords.");
            }

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
                user.PasswordUpdatedAt = DateTime.UtcNow;

                if (user.Status == StaffStatusEnum.CreatePassword)
                {
                    user.Status = StaffStatusEnum.Active;
                }

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
    }
}
