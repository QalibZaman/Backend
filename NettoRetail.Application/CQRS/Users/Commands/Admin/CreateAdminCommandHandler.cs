using Mapster;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Admin;

public record CreateAdminCommand(AdminSignUpModel Model) : IRequest<TokenResponse>;

public class CreateAdminCommandHandler(IAccountRepository accountRepository,
    IRoleRepository roleRepository,
    IJwtManager jwtManager,
    IOptions<AdminRoleOptions> adminOptions)
    : IRequestHandler<CreateAdminCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetAccountByEmailSpecification(request.Model.Email);
        var staff = await accountRepository.GetAsync(specification, cancellationToken);

        if (staff != null)
        {
            throw new ConflictException("This user is already registered.");
        }

        var admin = request.Model.Adapt<AccountEntity>();
        admin.RoleId = adminOptions.Value.RoleId;
        admin.Status = StaffStatusEnum.Online;

        admin.PasswordHash = PasswordHasher.HashPassword(admin.PasswordHash);
        var user = await accountRepository.AddAsync(admin, cancellationToken);

        var tokenResponse = jwtManager.GetTokenResponse(user.Id.ToString(), user.Email, user.RoleId.ToString(), user.ShopId);

        return tokenResponse;
    }
}