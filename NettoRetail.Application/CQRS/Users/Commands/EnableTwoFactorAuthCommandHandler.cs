using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.TwoFactorAuth;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record EnableTwoFactorAuthCommand(Guid UserId, EnableTwoFactorAuthRequest Model, string AccessToken)
    : IRequest<EnableTwoFactorAuthResponse>;

public class EnableTwoFactorAuthCommandHandler(
    IStaffRepository staffRepository,
    ITotpService totpService,
    IFailedAuthAttemptRepository loginAttemptRepository,
    IFailedAuthAttemptService loginAttemptService,
    IBlacklistTokenService blacklistTokenService,
    ISessionRepository sessionRepository,
    IOptions<UserBlockOptions> userBlockOptions)
    : IRequestHandler<EnableTwoFactorAuthCommand, EnableTwoFactorAuthResponse>
{
    private readonly int _failedAttemptThreshold = userBlockOptions.Value.FailedAttemptThreshold;

    public async Task<EnableTwoFactorAuthResponse> Handle(
        EnableTwoFactorAuthCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByIdSpecification(request.UserId);
        var user = await staffRepository.GetAsync(specification, cancellationToken)
                   ?? throw new NotFoundException("User not found.");

        var loginSpecification = new GetFailedAuthAttemptByStaffIdSpecification(user.Id);
        var loginAttempt = await loginAttemptRepository.GetAsync(loginSpecification, cancellationToken);

        if (!PasswordHasher.VerifyPassword(request.Model.ConfirmPassword, user.PasswordHash))
        {
            await loginAttemptService.IncrementFailedAttemptsAsync(loginAttempt!, user.Id, cancellationToken);

            int remainingAttempts = _failedAttemptThreshold - (loginAttempt?.FailedAttempts ?? 1);

            if (remainingAttempts == 0)
            {
                var sessionSpecification = new GetSessionsByJwtSpecification(request.AccessToken);
                await sessionRepository.DeleteAsync(sessionSpecification, cancellationToken);

                await blacklistTokenService.BlacklistTokenAsync(request.AccessToken, cancellationToken);

                throw new MyUnauthorizedException("Too many failed attempts. You have been logged out.");
            }

            throw new BadRequestException($"Password is not correct. {remainingAttempts} attempt(s) remaining before logout.");
        }

        if (loginAttempt != null)
        {
            await loginAttemptService.ResetFailedAttemptsAsync(loginAttempt, cancellationToken);
        }

        var rawSecret = totpService.GenerateSecret();
        var qrCodeUri = totpService.GenerateQrCodeUri(user.Email, rawSecret);
        var encryptedSecret = totpService.EncryptSecret(rawSecret);

        user.EncryptedTotpSecret = encryptedSecret;
        await staffRepository.UpdateAsync(user, cancellationToken);

        return new EnableTwoFactorAuthResponse(rawSecret, qrCodeUri);
    }
}
