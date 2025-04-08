using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.UOW;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record UpdateStaffCommand(Guid Id, UpdateStaffRequest Model, int ShopId) : IRequest;
public class UpdateStaffCommandHandler(
    IAccountRepository accountRepository,
    IRoleRepository roleRepository,
    ISessionTerminationService sessionTerminationService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateStaffCommand>
{
    public async Task Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var staffSpecification = new GetAccountByIdSpecification(request.Id);
        var staff = await accountRepository.GetAsync(staffSpecification, cancellationToken)
            ?? throw new NotFoundException("There is no staff member item with such id.");

        if (staff.ShopId != request.ShopId)
            throw new BadRequestException("You are not allowed to access this staff member.");

        if (request.Model.Email != null && staff.Email != request.Model.Email)
        {
            var emailConflictSpec = new GetAccountByEmailSpecification(request.Model.Email);
            var existingWithEmail = await accountRepository.GetAsync(emailConflictSpec, cancellationToken);
            if (existingWithEmail != null)
                throw new ConflictException("Another account with this email already exists.");
        }

        if (request.Model.RoleId != null)
        {
            var roleSpec = new GetRoleByIdSpecification(request.Model.RoleId, request.ShopId);
            _ = await roleRepository.GetAsync(roleSpec, cancellationToken)
                ?? throw new NotFoundException("Role not found.");
        }

        // begin transaction to ensure atomicity between status update, session termination, and staff data changes.
        // this prevents partial updates in case of failure — for example, if sessions are terminated but the staff update fails.
        var transaction = unitOfWork.BeginTransaction();
        try
        {
            if (request.Model.IsActive.HasValue)
            {
                if (request.Model.IsActive.Value && staff.Status == StaffStatusEnum.Deactivated)
                {
                    staff.Status = staff.PasswordUpdatedAt == null
                        ? StaffStatusEnum.CreatePassword
                        : StaffStatusEnum.Offline;
                }
                else if (!request.Model.IsActive.Value && staff.Status != StaffStatusEnum.Deactivated)
                {
                    staff.Status = StaffStatusEnum.Deactivated;
                    await sessionTerminationService.TerminateSessionsAsync(staff.Id, cancellationToken);
                }
            }

            request.Model.Adapt(staff);
            await accountRepository.UpdateAsync(staff, cancellationToken);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
