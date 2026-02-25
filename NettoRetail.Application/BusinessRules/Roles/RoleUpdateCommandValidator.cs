using FluentValidation;
using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Security;

namespace NettoRetail.Application.BusinessRules.Roles;

public class RoleUpdateCommandValidator : AbstractValidator<RoleUpdateCommand>
{
    public RoleUpdateCommandValidator(IRoleRepository roleRepository, IShopRepository shopRepository, IPermissionRepository permissionRepository)
    {
        // Shop existence validation
        RuleFor(x => x.ShopId)
            .MustAsync(async (shopId, ct) => await shopRepository.Exists(new GetShopByIdSpecification(shopId), ct))
            .WithMessage("Shop does not exist.");

        // Role existence validation
        RuleFor(x => x)
            .MustAsync(async (command, ct) => await roleRepository.Exists(
                    new GetRoleByShopIdSpecification(
                        command.Id,
                        command.ShopId),
                    ct))
            .WithMessage("Role does not exist.");

        // Unique role name validation
        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
                !await roleRepository.Exists(
                    new GetRoleByNameExcludingCurrentSpecification(
                        command.Model.Name,
                        command.Id),
                    ct))
            .WithMessage("Role with such name already exists.");

        // Permission existence batch validation
        RuleFor(x => x.Model.PermissionIds)
            .MustAsync(async (ids, ct) =>
            {
                var distinctIds = ids.Distinct().ToArray();

                var existingPermissionIds = await permissionRepository.GetIdsBySpecAsync(
                    new GetValidPermissionsSpecification(distinctIds),
                    ct);

                return existingPermissionIds.Count() == distinctIds.Length;
            })
            .WithMessage("One or more PermissionIds do not exist.");

        RuleFor(x => x.Model.PermissionIds)
            .Must(ids =>
            {
                var systemPermissionIds = PermissionRegistry.All
                    .Values
                    .Where(p => p.IsSystemPermission)
                    .Select(p => p.Id);

                return !ids.Intersect(systemPermissionIds).Any();
            })
            .WithMessage("System permissions cannot be assigned.");
    }
}