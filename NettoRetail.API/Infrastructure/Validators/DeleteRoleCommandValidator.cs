using FluentValidation;
using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.API.Infrastructure.Validators;

public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    private readonly IShopRepository _shopRepository;
    private readonly IStaffRepository _accountRepository;
    public DeleteRoleCommandValidator(IShopRepository shopRepository, IStaffRepository accountRepository)
    {
        _shopRepository = shopRepository;
        _accountRepository = accountRepository;
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .Custom(AdminCannotBeDeleted)
            .CustomAsync(ValidateAccount);

        RuleFor(x => x.ShopId)
            .CustomAsync(ValidateShop);
    }

    private void AdminCannotBeDeleted(int id, ValidationContext<DeleteRoleCommand> context)
    {
        if (id == RoleConstants.Admin || id == RoleConstants.SuperAdmin)
        {
            throw new ConflictException(ValidationErrorMessages.Role.CannotDeleteAdminRoleErrorMessage);
        }
    }

    private async Task ValidateAccount(int id, ValidationContext<DeleteRoleCommand> context, CancellationToken cancellationToken)
    {
        var accountExistsSpecification = new GetStaffByRoleIdSpecification(id);
        var accountExists = await _accountRepository.ExistsAsync(accountExistsSpecification, cancellationToken);
        if (accountExists)
        {
            throw new ConflictException(ValidationErrorMessages.Role.RoleInUseErrorMessage);
        }
    }

    private async Task ValidateShop(int shopId, ValidationContext<DeleteRoleCommand> context, CancellationToken cancellationToken)
    {
        var shopExists = await _shopRepository.ExistsAsync(new GetShopByIdSpecification(shopId), cancellationToken);
        if (!shopExists)
        {
            throw new NotFoundException(string.Format(ValidationErrorMessages.Shop.ShopDoesntExistErrorMessage, shopId));
        }
    }
}
