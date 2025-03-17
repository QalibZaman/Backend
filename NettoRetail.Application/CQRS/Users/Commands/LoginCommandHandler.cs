using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;
namespace NettoRetail.Application.CQRS.Users.Commands;

public record LoginCommand(LoginModel Model) : IRequest<TokenResponse>;

public class LoginCommandHandler(IStaffRepository repository,
    IJwtManager jwtManager,
    IFailedAuthAttemptRepository loginAttemptRepository,
    IFailedAuthAttemptService loginAttemptService)
    : IRequestHandler<LoginCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByEmailSpecification(request.Model.Email);
        var user = await repository.GetAsync(specification, cancellationToken) ??
            throw new MyUnauthorizedException("User was not found");

        var loginSpecification = new GetFailedAuthAttemptByStaffIdSpecification(user.Id);
        var loginAttempt = await loginAttemptRepository.GetAsync(loginSpecification, cancellationToken);

        if (loginAttemptService.IsAccountLocked(loginAttempt!))
        {
            throw new MyUnauthorizedException("Account is locked. Try again later.");
        }

        if (loginAttemptService.HasBlockDurationElapsed(loginAttempt!))
        {
            await loginAttemptService.ResetFailedAttemptsAsync(loginAttempt!, cancellationToken);
        }

        if (!PasswordHasher.VerifyPassword(request.Model.Password, user.PasswordHash))
        {
            await loginAttemptService.IncrementFailedAttemptsAsync(loginAttempt!, user.Id, cancellationToken);
            throw new MyUnauthorizedException("Invalid email or password.");
        }

        if (loginAttempt != null)
        {
            await loginAttemptService.ResetFailedAttemptsAsync(loginAttempt, cancellationToken);
        }

        if (user.TwoFactorEnabled)
        {
            var tempToken = jwtManager.GenerateTempToken(user.Id.ToString());
            return new TokenResponse(null, null, tempToken);
        }

        var tokenResponse = jwtManager.GetTokenResponse(user.Id.ToString(), user.Email, user.RoleId.ToString());

        return tokenResponse;
    }
}