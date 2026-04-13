using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.TwoFactorAuth;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record DisableTwoFactorAuthCommand(Guid UserId, DisableTwoFactorAuthRequest Model, string AccessToken) : IRequest;

public class DisableTwoFactorAuthCommandHandler(
    IStaffRepository userRepository,
    IFailedAuthAttemptRepository loginAttemptRepository,
    IFailedAuthAttemptService loginAttemptService,
    IBlacklistTokenService blacklistTokenService,
    ISessionRepository sessionRepository,
    IOptions<UserBlockOptions> userBlockOptions)
    : IRequestHandler<DisableTwoFactorAuthCommand>
{
    private readonly int _failedAttemptThreshold = userBlockOptions.Value.FailedAttemptThreshold;

    public async Task Handle(DisableTwoFactorAuthCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByIdSpecification(request.UserId);
        var user = await userRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException("User not found.");

        if (!user.TwoFactorEnabled)
            throw new BadRequestException("Two-factor authentication is already disabled for this account.");

        var loginSpecification = new GetFailedAuthAttemptByAccountIdSpecification(user.Id);
        var loginAttempt = await loginAttemptRepository.GetAsync(loginSpecification, cancellationToken);

        if (!PasswordHasher.VerifyPassword(request.Model.ConfirmPassword, user.PasswordHash))
        {
            await loginAttemptService.IncrementFailedAttemptsAsync(loginAttempt!, user.Id, cancellationToken);

            int remainingAttempts = _failedAttemptThreshold - (loginAttempt?.FailedAttempts ?? 1);

            if (remainingAttempts == 0)
            {
                var sessionSpecification = new GetSessionsByJwtSpecification(request.AccessToken);
                await sessionRepository.DeleteAsync(sessionSpecification, cancellationToken);

                var expiresAt = JwtUtils.GetJwtExpiration(request.AccessToken ?? throw new InvalidTokenCredentialsException("Invalid JWT or missing expiration claim."));
                await blacklistTokenService.BlacklistTokenAsync(request.AccessToken, expiresAt!.Value, cancellationToken);

                throw new MyUnauthorizedException("Too many failed attempts. You have been logged out.");
            }

            throw new BadRequestException($"Password is not correct. {remainingAttempts} attempt(s) remaining before logout.");
        }

        if (loginAttempt != null)
            await loginAttemptService.ResetFailedAttemptsAsync(loginAttempt, cancellationToken);

        user.TwoFactorEnabled = false;
        user.EncryptedTotpSecret = null;

        await userRepository.UpdateAsync(user, cancellationToken);
    }
}
