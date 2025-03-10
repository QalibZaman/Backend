using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;
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
        var claimsPrincipal = jwtManager.Validate(request.RefreshToken) ??
            throw new MyUnauthorizedException("The refresh token is invalid");

        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            throw new MyUnauthorizedException("The refresh token is invalid");

        var isParsed = Guid.TryParse(userId, out Guid parsedUserId);

        if (!isParsed)
        {
            throw new MyUnauthorizedException("The refresh token is invalid");
        }

        var specification = new GetStaffByIdSpecification(parsedUserId);
        var user = await repository.GetAsync(specification, cancellationToken) ??
            throw new NotFoundException("User not found.");

        var accessTokenClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.RoleId.ToString()),
            new("token_type", "access")
        };

        if (user.ShopId != null)
        {
            accessTokenClaims.Add(new Claim("shop_id", user.ShopId.Value.ToString()));
        }

        var newAccessToken = jwtManager.Generate(accessTokenClaims, TimeSpan.FromMinutes(accessTokenExpirationMinutes));
        var newRefreshToken = jwtManager.Generate(claimsPrincipal.Claims.ToList(), TimeSpan.FromDays(refreshTokenExpirationDays));

        var tokenResponse = new TokenResponse(newAccessToken, newRefreshToken);

        return tokenResponse;
    }
}