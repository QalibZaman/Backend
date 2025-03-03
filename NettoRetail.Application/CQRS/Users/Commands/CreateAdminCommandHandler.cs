using System.Security.Claims;
using Mapster;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record CreateAdminCommand(AdminSignUpModel Model) : IRequest<TokenResponse>;

public class CreateAdminCommandHandler(IStaffRepository staffRepository, IRoleRepository roleRepository, IJwtManager jwtManager, IOptions<JwtOptions> jwtOptions)
    : IRequestHandler<CreateAdminCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
    {
        var admin = request.Model.Adapt<StaffEntity>();

        var roleByNameSpecification = new GetRoleByNameSpecification(RoleEnum.Admin);
        var role = await roleRepository.GetAsync(roleByNameSpecification, cancellationToken);

        admin.RoleId = role!.Id;
        admin.PasswordHash = PasswordHasher.HashPassword(admin.PasswordHash);

        var user = await staffRepository.AddAsync(admin, cancellationToken);

        var accessTokenClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.RoleId.ToString()),
            new("token_type", "access")
        };
        var refreshTokenClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("token_type", "refresh")
        };

        var accessToken = jwtManager.Generate(accessTokenClaims, TimeSpan.FromMinutes(jwtOptions.Value.AccessTokenExpirationMinutes));
        var refreshToken = jwtManager.Generate(refreshTokenClaims, TimeSpan.FromDays(jwtOptions.Value.RefreshTokenExpirationDays));

        var tokenResponse = new TokenResponse(accessToken, refreshToken);

        return tokenResponse;
    }
}