using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record ConfirmTwoFactorAuthCommand(Guid UserId, string TotpCode) : IRequest;

public class ConfirmTwoFactorAuthCommandHandler(
    IStaffRepository userRepository,
    ITotpService totpService)
    : IRequestHandler<ConfirmTwoFactorAuthCommand>
{
    public async Task Handle(ConfirmTwoFactorAuthCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TotpCode))
            throw new BadRequestException("2FA code must not be empty.");

        var specification = new GetStaffByIdSpecification(request.UserId);
        var user = await userRepository.GetAsync(specification, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        if (!totpService.VerifyCode(user.EncryptedTotpSecret, request.TotpCode))
        {
            throw new BadRequestException("Invalid 2FA code.");
        }

        user.TwoFactorEnabled = true;
        await userRepository.UpdateAsync(user, cancellationToken);
    }
}
