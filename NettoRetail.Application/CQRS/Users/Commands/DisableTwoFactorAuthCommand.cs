using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record DisableTwoFactorAuthCommand(Guid UserId, string TotpCode) : IRequest;

public class DisableTwoFactorAuthCommandHandler(
    IAccountRepository userRepository,
    ITotpService totpService)
    : IRequestHandler<DisableTwoFactorAuthCommand>
{
    public async Task Handle(DisableTwoFactorAuthCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TotpCode))
            throw new BadRequestException("2FA code must not be empty.");

        var specification = new GetAccountByIdSpecification(request.UserId);
        var user = await userRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException("User not found.");

        if (!user.TwoFactorEnabled)
            throw new BadRequestException("Two-factor authentication is already disabled for this account.");

        if (!totpService.VerifyCode(user.EncryptedTotpSecret, request.TotpCode))
            throw new BadRequestException("Invalid 2FA code.");

        user.TwoFactorEnabled = false;
        user.EncryptedTotpSecret = null;

        await userRepository.UpdateAsync(user, cancellationToken);
    }
}
