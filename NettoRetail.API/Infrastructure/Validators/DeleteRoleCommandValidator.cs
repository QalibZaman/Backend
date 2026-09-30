using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Infrastructure.Validators;

public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    private readonly IShopRepository _shopRepository;
    private readonly IStaffRepository _staffRepository;
    public DeleteRoleCommandValidator(IShopRepository shopRepository, IStaffRepository staffRepository)
    {
        _shopRepository = shopRepository;
        _staffRepository = staffRepository;
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        RuleFor(x => x.id)
            .GreaterThan(0)
            .Custom(AdminCannotBeDeleted)
            .CustomAsync(ValidateAccount);

        RuleFor(x => x.shopId)
            .CustomAsync(ValidateShop);
    }

    private void AdminCannotBeDeleted(int id, ValidationContext<DeleteRoleCommand> context)
    {
        if (id == RoleConstants.Admin || id == RoleConstants.SuperAdmin)
        {
            throw new ConflictException("The admin role cannot be deleted.");
        }
    }

    private async Task ValidateAccount(int id, ValidationContext<DeleteRoleCommand> context, CancellationToken cancellationToken)
    {
        var accountExistsSpecification = new GetStaffByRoleIdSpecification(id);
        var accountExists = await _staffRepository.ExistsAsync(accountExistsSpecification, cancellationToken);
        if (accountExists)
        {
            throw new ConflictException("Cannot delete role. This role is currently assigned to active staff. Please reassign or remove staff before deletion.");
        }
    }

    private async Task ValidateShop(int shopId, ValidationContext<DeleteRoleCommand> context, CancellationToken cancellationToken)
    {
        var shopExists = await _shopRepository.ExistsAsync(new GetShopByIdSpecification(shopId), cancellationToken);
        if (!shopExists)
        {
            throw new NotFoundException(string.Format("No shop with Id: {0}", shopId));
        }
    }
}
