using System.ComponentModel.DataAnnotations;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Identity;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record CreateAdminCommand(AdminSignUpModel Model) : IRequest<TokenResponse>;
public class CreateAdminCommandHandler(
    IStaffRepository staffRepository,
    UserManager<ApplicationUser> userManager,
    IJwtManager jwtManager,
    IOptions<AdminRoleOptions> adminOptions,
    IMapper mapper)
    : IRequestHandler<CreateAdminCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Model.Email);

        if (existingUser != null)
        {
            throw new ConflictException(HandlerConstants.Admin.AccountAlreadyExistsMessage);
        }

        var id = Guid.NewGuid();

        var identityUser = new ApplicationUser
        {
            Id = id,
            UserName = request.Model.Email,
            Email = request.Model.Email
        };

        var identityResult = await userManager.CreateAsync(identityUser, request.Model.Password);

        if (!identityResult.Succeeded)
        {
            throw new ValidationException(
                string.Join(", ", identityResult.Errors.Select(e => e.Description)));
        }

        var admin = mapper.Map<StaffEntity>(request.Model);
        admin.Id = id;
        admin.RoleId = adminOptions.Value.RoleId;
        admin.Status = StaffStatusEnum.Active;
        var user = await staffRepository.AddAsync(admin, cancellationToken);

        var tokenResponse = jwtManager.GetTokenResponse(user.Id.ToString(), user.Email, user.RoleId.ToString(), user.ShopId);
        return tokenResponse;
    }
}