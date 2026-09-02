using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Services;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.Services.IPasswordService
{
    public class PasswordService(
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        ILogger<PasswordService> logger
        ) : IPasswordService
    {
        public async Task<bool> ValidatePasswordAsync(Guid userId, string password, CancellationToken cancellationToken)
        {
            var user = await GetUserAsync(userId, cancellationToken);
            return PasswordHasher.VerifyPassword(password, user.PasswordHash);
        }

        public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
        {
            var user = await GetUserAsync(userId, cancellationToken);

            var transaction = unitOfWork.BeginTransaction();
            try
            {
                user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                user.PasswordUpdatedAt = DateTime.UtcNow;

                await staffRepository.UpdateAsync(user, cancellationToken);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            logger.LogInformation("Password changed for user {UserId}", userId);
        }

        private async Task<StaffEntity> GetUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var spec = new GetStaffByIdSpecification(userId);
            return await staffRepository.GetAsync(spec, cancellationToken)
                   ?? throw new NotFoundException("User not found.");
        }
    }
}