using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IStaffService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.UOW;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record UpdateStaffCommand(Guid Id, UpdateStaffRequest Model, int ShopId) : IRequest;
public class UpdateStaffCommandHandler(
    IAccountRepository accountRepository,
    IRoleRepository roleRepository,
    IStaffService staffService,
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

        await CheckRoleExistenceAsync(request.Model, request.ShopId, cancellationToken);

        var transaction = unitOfWork.BeginTransaction();
        try
        {
            await staffService.UpdateStaffAsync(request.Model, staff, cancellationToken);

            await accountRepository.UpdateAsync(staff, cancellationToken);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private async Task CheckRoleExistenceAsync(
        UpdateStaffRequest request,
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
}
