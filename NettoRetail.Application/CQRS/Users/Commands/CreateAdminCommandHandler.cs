using Mapster;
using MediatR;
using NettoRetail.Application.Models.Entities;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record CreateAdminCommand(AdminSignUpModel Model) : IRequest;

public class CreateAdminCommandHandler(IStaffRepository staffRepository)
    : IRequestHandler<CreateAdminCommand>
{
    public async Task Handle(CreateAdminCommand request, CancellationToken cancellationToken)
    {
        var admin = request.Model.Adapt<StaffEntity>();
        admin.RoleId = RoleIdConstants.AdminId;
        admin.PasswordHash = PasswordHasher.HashPassword(admin.PasswordHash);

        await staffRepository.AddAsync(admin, cancellationToken);
    }
}