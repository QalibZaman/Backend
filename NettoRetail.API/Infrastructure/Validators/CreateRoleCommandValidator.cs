using FluentValidation;
using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    private readonly IValidator<CreateRoleRequest> _validator;
    private readonly IShopRepository _shopRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    public CreateRoleCommandValidator(IValidator<CreateRoleRequest> validator, IShopRepository shopRepository, IRoleRepository roleRepository, IPermissionRepository permissionRepository)
    {
        _validator = validator;
        _shopRepository = shopRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Model)
            .SetValidator(_validator);

        RuleFor(x => x.ShopId)
            .GreaterThan(0).WithMessage(ValidationErrorMessages.Shop.ShopIdRequiredErrorMessage)
            .CustomAsync(ValidateShopExistence);

        RuleFor(x => x)
            .CustomAsync(ValidateRoleNameUniqueness);

        RuleFor(x => x.Model.PermissionIds)
            .CustomAsync(ValidatePermissions);
    }

    private async Task ValidateShopExistence(int shopId, ValidationContext<CreateRoleCommand> context, CancellationToken cancellationToken)
    {
        var shopExists = await _shopRepository.ExistsAsync(new GetShopByIdSpecification(shopId), cancellationToken);
        if (!shopExists)
            throw new NotFoundException(string.Format(ValidationErrorMessages.Shop.ShopDoesntExistErrorMessage, shopId));
    }

    private async Task ValidateRoleNameUniqueness(CreateRoleCommand command, ValidationContext<CreateRoleCommand> context, CancellationToken cancellationToken)
    {
        var roleExists = await _roleRepository.ExistsAsync(new GetRoleByNameAndShopIdSpecification(command.Model.Name, command.ShopId), cancellationToken);
        if (roleExists)
            throw new ConflictException(ValidationErrorMessages.Role.RoleExistsErrorMessage);
    }

    private async Task ValidatePermissions(int[] permissionIds, ValidationContext<CreateRoleCommand> context, CancellationToken cancellationToken)
    {
        var distinctPermissionIds = permissionIds.Distinct().ToArray();
        var validSpec = new GetValidPermissionsSpecification(distinctPermissionIds);
        var validPermissions = await _permissionRepository.GetIdsBySpecAsync(validSpec, cancellationToken);

        if (validPermissions.Count() != distinctPermissionIds.Length)
            throw new NotFoundException(ValidationErrorMessages.Role.PermissionsDontExistErrorMessage);
    }
}
