using Mapster;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Admin;

public record CreateAdminCommand(AdminSignUpModel Model) : IRequest<TokenResponse>;

public class CreateAdminCommandHandler(IStaffRepository staffRepository,
    IJwtManager jwtManager,
    IOptions<AdminRoleOptions> adminOptions)
    : IRequestHandler<CreateAdminCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByEmailSpecification(request.Model.Email);
        if (await staffRepository.Exists(specification, cancellationToken))
            throw new ConflictException("This user is already registered.");

        var admin = request.Model.Adapt<StaffEntity>();
        admin.RoleId = adminOptions.Value.RoleId;
        admin.Status = StaffStatusEnum.Online;

        admin.CommunicationPreference = CommunicationPreferencesSetter.SetCommunicationPreferences(admin);

        admin.PasswordHash = PasswordHasher.HashPassword(admin.PasswordHash);
        var user = await staffRepository.AddAsync(admin, cancellationToken);

        var tokenResponse = jwtManager.GetTokenResponse(user.Id.ToString(), user.Email, user.RoleId.ToString(), user.ShopId);

        return tokenResponse;
    }
}