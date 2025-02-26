using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<TokenResponse>;

public class RefreshTokenCommandHandler(IJwtManager jwtManager)
    : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    public Task<TokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var claimsPrincipal = jwtManager.Validate(request.RefreshToken);

        if (claimsPrincipal == null)
        {
            throw new MyUnauthorizedException("The refresh token is invalid");
        }

        var newAccessToken = jwtManager.Generate(claimsPrincipal.Claims.ToList(), TimeSpan.FromMinutes(30));
        var newRefreshToken = jwtManager.Generate(claimsPrincipal.Claims.ToList(), TimeSpan.FromDays(7));

        var tokenResponse = new TokenResponse(newAccessToken, newRefreshToken);

        return Task.FromResult(tokenResponse);
    }
}