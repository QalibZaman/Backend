using FluentValidation;
using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.Exceptions;
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
            .CustomAsync(async (shopId, context, ct) =>
            {
                var exists = await shopRepository.Exists(new GetShopByIdSpecification(shopId), ct);

                if (!exists)
                    throw new NotFoundException($"Shop with id {shopId} does not exist.");
            });

        // Role existence validation
        RuleFor(x => x)
            .CustomAsync(async (command, context, ct) =>
            {
                var exists = await roleRepository.Exists(
                    new GetRoleByShopIdSpecification(
                        command.Id,
                        command.ShopId),
                    ct);

                if (!exists)
                    throw new NotFoundException($"Role with Id {command.Id} and ShopId {command.ShopId} does not exist.");
            });

        // Unique role name validation
        RuleFor(x => x)
            .CustomAsync(async (command, context, ct) =>
            {
                var exists = await roleRepository.Exists(
                    new GetRoleByNameExcludingCurrentSpecification(
                        command.Model.Name,
                        command.Id),
                    ct);

                if (exists)
                    throw new ConflictException("Role with such name already exists.");
            });

        // Permission existence batch validation
        RuleFor(x => x.Model.PermissionIds)
            .CustomAsync(async (ids, context, ct) =>
            {
                var distinctIds = ids.Distinct().ToArray();

                var existingPermissionIds = await permissionRepository
                    .GetIdsBySpecAsync(
                        new GetValidPermissionsSpecification(distinctIds),
                        ct);

                if (existingPermissionIds.Count() != distinctIds.Length)
                    throw new BadRequestException("One or more PermissionIds do not exist.");
            });

        RuleFor(x => x.Model.PermissionIds)
            .Custom((ids, context) =>
            {
                var systemPermissionIds = PermissionRegistry.All
                    .Values
                    .Where(p => p.IsSystemPermission)
                    .Select(p => p.Id);

                if (ids.Intersect(systemPermissionIds).Any())
                    throw new ForbiddenException("System permissions cannot be assigned.");
            });
    }
}