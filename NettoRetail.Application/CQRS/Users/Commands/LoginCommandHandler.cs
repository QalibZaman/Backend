using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Utilities;
namespace NettoRetail.Application.CQRS.Users.Commands;

public record LoginCommand(LoginModel Model) : IRequest<TokenResponse>;

public class LoginCommandHandler(IStaffRepository repository, IJwtManager jwtManager, IOptions<JwtOptions> jwtOptions)
    : IRequestHandler<LoginCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByEmailSpecification(request.Model.Email);
        var user = await repository.GetAsync(specification, cancellationToken);
        var accessTokenExpirationMinutes = jwtOptions.Value.AccessTokenExpirationMinutes;
        var refreshTokenExpirationDays = jwtOptions.Value.RefreshTokenExpirationDays;

        if (user == null) throw new MyUnauthorizedException("User was not found");

        if (!PasswordHasher.VerifyPassword(request.Model.Password, user.PasswordHash))
        {
            throw new MyUnauthorizedException("Password is incorrect");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.RoleId.ToString())
        };

        var accessToken = jwtManager.Generate(claims, TimeSpan.FromMinutes(accessTokenExpirationMinutes));
        var refreshToken = jwtManager.Generate(claims, TimeSpan.FromDays(refreshTokenExpirationDays));

        var tokenResponse = new TokenResponse(accessToken, refreshToken);

        return tokenResponse;
    }
}