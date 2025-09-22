using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.ISessionModelFactory;
using NettoRetail.Application.Services.PermissionCache;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.PermissionRole;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record CreateSessionCommand(string Ips, string UserAgent, string Jwt, string RefreshToken) : IRequest;

public class CreateSessionCommandHandler(
    ISessionRepository sessionRepository,
    IAccountRepository accountRepository,
    IPermissionRoleRepository permissionRoleRepository,
    ISessionModelFactory sessionModelFactory,
    IIpInfoService ipInfoService,
    IPermissionCacheService permissionCacheService) : IRequestHandler<CreateSessionCommand>
{
    public async Task Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var ip = request.Ips.Split(',')[0].Trim();

        Location? location;

        if (ip == string.Empty)
            location = null;
        else
            location = await ipInfoService.GetIpInfoAsync(ip);

        var expiresAt = JwtUtils.GetJwtExpiration(request.Jwt) ?? throw new InvalidTokenCredentialsException("Invalid JWT or missing expiration claim.");
        var sessionModel = await sessionModelFactory.CreateSessionModel(request.UserAgent, ip, request.Jwt, request.RefreshToken, location, expiresAt, cancellationToken);

        var session = sessionModel.Adapt<SessionEntity>();

        try
        {
            await sessionRepository.AddAsync(session, cancellationToken);

            var accSpecification = new GetAccountByIdSpecification(session.AccountId);
            var account = await accountRepository.GetAsync(accSpecification, cancellationToken) ?? throw new NotFoundException("Account not found");
            var roleId = account.RoleId;

            var permissionSpecification = new GetPermissionsByRoleIdSpecification(roleId);
            var userPermissions = await permissionRoleRepository.GetPermissionsByRoleIdAsync(permissionSpecification, cancellationToken) ?? throw new BadRequestException("No permissions");

            permissionCacheService.SetPermissions(userPermissions, session.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}