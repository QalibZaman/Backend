using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<TokenResponse>;

public class RefreshTokenCommandHandler(IStaffRepository repository, IJwtManager jwtManager, IOptions<JwtOptions> jwtOptions)
    : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var accessTokenExpirationMinutes = jwtOptions.Value.AccessTokenExpirationMinutes;
        var refreshTokenExpirationDays = jwtOptions.Value.RefreshTokenExpirationDays;
        var claimsPrincipal = jwtManager.Validate(request.RefreshToken);

        if (claimsPrincipal == null)
        {
            throw new MyUnauthorizedException("The refresh token is invalid");
        }

        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            throw new MyUnauthorizedException("The refresh token is invalid");
        }

        var specification = new GetStaffByIdSpecification(userId);
        var user = await repository.GetAsync(specification, cancellationToken);
        var accessTokenClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.RoleId.ToString()),
            new("token_type", "access")
        };

        var newAccessToken = jwtManager.Generate(accessTokenClaims, TimeSpan.FromMinutes(accessTokenExpirationMinutes));
        var newRefreshToken = jwtManager.Generate(claimsPrincipal.Claims.ToList(), TimeSpan.FromDays(refreshTokenExpirationDays));

        var tokenResponse = new TokenResponse(newAccessToken, newRefreshToken);

        return tokenResponse;
    }
}