using MediatR;
using NettoHealth.Application.Exceptions;
using NettoHealth.Application.Models.Response;
using NettoHealth.Shared.Interfaces.Utilities;

namespace NettoHealth.Application.CQRS.Users.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<ServiceResponse<TokenResponse>>;

public class RefreshTokenCommandHandler(IJwtManager jwtManager)
    : IRequestHandler<RefreshTokenCommand, ServiceResponse<TokenResponse>>
{
    public Task<ServiceResponse<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {

        var claimsPrincipal = jwtManager.ValidateRefreshToken(request.RefreshToken);

        if (claimsPrincipal == null) throw new MyUnauthorizedException("The refresh token is invalid");

        var newAccessToken = jwtManager.Generate(claimsPrincipal.Claims.ToList(), TimeSpan.FromMinutes(30));
        var newRefreshToken = jwtManager.Generate(claimsPrincipal.Claims.ToList(), TimeSpan.FromDays(7));

        var tokenResponse = new TokenResponse(newAccessToken, newRefreshToken);

        return Task.FromResult(new ServiceResponse<TokenResponse>("Token refreshed successfully", tokenResponse, true));
    }
}