using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record VerifyTwoFactorAuthCommand(Guid UserId, string TotpCode) : IRequest<TokenResponse>;

public class VerifyTwoFactorAuthCommandHandler(
    IStaffRepository userRepository,
    ITotpService totpService,
    IJwtManager jwtManager)
    : IRequestHandler<VerifyTwoFactorAuthCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(VerifyTwoFactorAuthCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(new GetStaffByIdSpecification(request.UserId), cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        if (!totpService.VerifyCode(user.EncryptedTotpSecret, request.TotpCode))
        {
            throw new MyUnauthorizedException("Invalid 2FA code.");
        }

        return jwtManager.GetTokenResponse(user.Id.ToString(), user.Email, user.RoleId.ToString(), user.ShopId);
    }
}
