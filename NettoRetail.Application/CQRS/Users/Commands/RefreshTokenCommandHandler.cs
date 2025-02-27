using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<TokenResponse>;

public class RefreshTokenCommandHandler(IJwtManager jwtManager, IOptions<JwtOptions> jwtOptions)
    : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    public Task<TokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var accessTokenExpirationMinutes = jwtOptions.Value.AccessTokenExpirationMinutes;
        var refreshTokenExpirationDays = jwtOptions.Value.RefreshTokenExpirationDays;
        var claimsPrincipal = jwtManager.Validate(request.RefreshToken);

        if (claimsPrincipal == null)
        {
            throw new MyUnauthorizedException("The refresh token is invalid");
        }

        var newAccessToken = jwtManager.Generate(claimsPrincipal.Claims.ToList(), TimeSpan.FromMinutes(accessTokenExpirationMinutes));
        var newRefreshToken = jwtManager.Generate(claimsPrincipal.Claims.ToList(), TimeSpan.FromDays(refreshTokenExpirationDays));

        var tokenResponse = new TokenResponse(newAccessToken, newRefreshToken);

        return Task.FromResult(tokenResponse);
    }
}