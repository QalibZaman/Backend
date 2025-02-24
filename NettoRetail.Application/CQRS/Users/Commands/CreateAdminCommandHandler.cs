using Mapster;
using MediatR;
using NettoRetail.Application.Models.Entities;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record CreateAdminCommand(AdminSignUpModel Model) : IRequest;

public class CreateAdminCommandHandler(IStaffRepository staffRepository, IRoleRepository roleRepository)
    : IRequestHandler<CreateAdminCommand>
{
    public async Task Handle(CreateAdminCommand request, CancellationToken cancellationToken)
    {
        var admin = request.Model.Adapt<StaffEntity>();

        var roleByNameSpecification = new GetRoleByNameSpecification(RoleEnum.Admin);
        var role = await roleRepository.GetAsync(roleByNameSpecification, cancellationToken);

        admin.RoleId = role!.Id;
        admin.PasswordHash = PasswordHasher.HashPassword(admin.PasswordHash);

        await staffRepository.AddAsync(admin, cancellationToken);
    }
}