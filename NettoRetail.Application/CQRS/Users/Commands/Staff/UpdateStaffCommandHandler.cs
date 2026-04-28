using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IStaffService;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record UpdateStaffCommand(Guid Id, UpdateStaffRequest Model, int ShopId) : IRequest;
public class UpdateStaffCommandHandler(
    IStaffRepository staffRepository,
    IRoleRepository roleRepository,
    IStaffService staffService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateStaffCommand>
{
    public async Task Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = await FetchAndAuthorizeStaffAsync(request.Id, request.ShopId, cancellationToken);

        await EnsureRoleExistsAsync(request.Model.RoleId, request.ShopId, cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();

        await staffService.UpdateStaffAsync(request.Model, staff, cancellationToken);

        await staffRepository.UpdateAsync(staff, cancellationToken);

        transaction.Commit();
    }

    private async Task<StaffEntity> FetchAndAuthorizeStaffAsync(Guid staffId, int shopId, CancellationToken cancellationToken)
    {
        var spec = new GetStaffByIdSpecification(staffId);
        var staff = await staffRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException(StaffConstants.StaffMemberNotFound);

        if (staff.ShopId != shopId)
            throw new ForbiddenException(StaffConstants.StaffMemberAccessDenied);

        return staff;
    }

    private async Task EnsureRoleExistsAsync(
        int? roleId,
        int shopId,
        CancellationToken cancellationToken)
    {
        if (roleId != null)
        {
            var spec = new GetRoleByShopIdOrAdminIdSpecification(roleId, shopId);
            var exists = await roleRepository.Exists(spec, cancellationToken);

            if (!exists)
                throw new NotFoundException(ValidationErrorMessages.Role.RoleNotFoundErrorMessage);
        }
    }
}
