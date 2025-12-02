using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.UOW;
using NettoRetail.Domain.Entities;
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

        await CheckRoleExistenceAsync(request.Model, staff, request.ShopId, cancellationToken);

        var transaction = unitOfWork.BeginTransaction();
        try
        {
            CheckIfDeactivated(request.Model, staff);

            await UpdateEmailAsync(request.Model, staff, cancellationToken);

            await UpdateActiveStatusAsync(request.Model, staff, cancellationToken);

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

    private void CheckIfDeactivated(
        UpdateStaffRequest request,
        AccountEntity staff)
    {
        if (staff.Status == StaffStatusEnum.Deactivated &&
                (!request.IsActive.HasValue || request.IsActive.Value == false))
        {
            throw new BadRequestException("You cannot edit a deactivated staff member unless you are reactivating them.");
        }
    }

    private async Task CheckRoleExistenceAsync(
        UpdateStaffRequest request,
        AccountEntity staff,
        int shopId,
        CancellationToken cancellationToken)
    {
        if (request.RoleId != null)
        {
            var roleSpec = new GetRoleByShopIdOrAdminIdSpecification(request.RoleId, shopId);
            var roleExists = await roleRepository.Exists(roleSpec, cancellationToken);

            if (!roleExists)
                throw new NotFoundException("Role not found.");
        }
    }

    private async Task UpdateEmailAsync(
        UpdateStaffRequest request,
        AccountEntity staff,
        CancellationToken cancellationToken)
    {
        if (request.Email == null || staff.Email == request.Email || staff.PasswordUpdatedAt == null)
            return;

        var emailConflictSpec = new GetAccountByEmailSpecification(request.Email);
        var existingWithEmail = await accountRepository.Exists(emailConflictSpec, cancellationToken);
        if (existingWithEmail)
            throw new ConflictException("Another account with this email already exists.");

        await TerminateSessionAsync(staff.Id, cancellationToken);

        staff.Status = StaffStatusEnum.Offline;
    }

    private async Task UpdateActiveStatusAsync(
        UpdateStaffRequest request,
        AccountEntity staff,
        CancellationToken cancellationToken)
    {
        if (!request.IsActive.HasValue)
        {
            return;
        }

        if (request.IsActive.Value && staff.Status == StaffStatusEnum.Deactivated)
        {
            staff.Status = staff.PasswordUpdatedAt == null
                ? StaffStatusEnum.CreatePassword
                : StaffStatusEnum.Offline;
        }
        else if (!request.IsActive.Value && staff.Status != StaffStatusEnum.Deactivated)
        {
            staff.Status = StaffStatusEnum.Deactivated;
            await TerminateSessionAsync(staff.Id, cancellationToken);
        }
    }

    private async Task TerminateSessionAsync(Guid id, CancellationToken cancellationToken)
    {
        await sessionTerminationService.TerminateSessionsAsync(id, cancellationToken);
    }
}
