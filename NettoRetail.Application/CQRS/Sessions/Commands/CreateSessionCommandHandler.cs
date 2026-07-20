using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.ISessionModelFactory;
using NettoRetail.Application.Services.PermissionCache;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.PermissionRole;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record CreateSessionCommand(string Ips,
    string UserAgent,
    string Jwt,
    string RefreshToken,
    UserTypeEnum UserType) : IRequest;

public class CreateSessionCommandHandler(
    ISessionRepository sessionRepository,
    IStaffRepository adminRepository,
    ICustomerRepository customerRepository,
    IPermissionRoleRepository permissionRoleRepository,
    ISessionModelFactory sessionModelFactory,
    IIpInfoService ipInfoService,
    IPermissionCacheService permissionCacheService,
    IMapper mapper) : IRequestHandler<CreateSessionCommand>
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
        var sessionModel = await sessionModelFactory.CreateSessionModel(request.UserAgent, ip, request.Jwt, request.RefreshToken, location, expiresAt, request.UserType, cancellationToken);

        var session = mapper.Map<SessionEntity>(sessionModel);

        await sessionRepository.AddAsync(session, cancellationToken);

        BaseUserEntity account;

        if (request.UserType == UserTypeEnum.Admin)
        {
            var accSpecification = new GetStaffByIdSpecification(session.StaffId);
            account = await adminRepository.GetAsync(accSpecification, cancellationToken)
                ?? throw new NotFoundException("Account not found");
        }
        else
        {
            var customerSpec = new GetCustomerByIdSpecification(session.StaffId);
            account = await customerRepository.GetAsync(customerSpec, cancellationToken)
                ?? throw new NotFoundException("Account not found");
        }

        var roleId = account.RoleId;

        var permissionSpecification = new GetPermissionsByRoleIdSpecification(roleId);
        var userPermissions = await permissionRoleRepository.GetPermissionsByRoleIdAsync(permissionSpecification, cancellationToken) ?? throw new BadRequestException("No permissions");

        permissionCacheService.SetPermissions(userPermissions, session.Id);
    }
}
