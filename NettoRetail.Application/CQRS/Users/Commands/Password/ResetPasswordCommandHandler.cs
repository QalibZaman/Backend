using System.Security.Claims;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Password;

public record ResetPasswordCommand(ResetPasswordRequest Model) : IRequest;
public class ResetPasswordCommandHandler(
    IAccountRepository accountRepository,
    IJwtManager jwtManager)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var claimsPrincipal = jwtManager.Validate(request.Model.Token) ??
            throw new InvalidTokenCredentialsException("Invalid or expired reset token.");

        var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value ??
            throw new BadRequestException("Invalid token payload.");

        var specification = new GetAccountByEmailSpecification(email);
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

        user.PasswordHash = PasswordHasher.HashPassword(request.Model.NewPassword);
        user.PasswordUpdatedAt = DateTime.UtcNow;

        if (user.Status == StaffStatusEnum.CreatePassword)
        {
            user.Status = StaffStatusEnum.Offline;
        }

        await accountRepository.UpdateAsync(user, cancellationToken);
    }
}
