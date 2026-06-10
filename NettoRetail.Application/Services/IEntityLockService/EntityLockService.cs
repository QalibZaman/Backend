using NettoRetail.Application.Enums;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.UtilityModels;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.IEntityLockService;

public class EntityLockService(
    ILockProvider lockProvider,
    IRoleRepository roleRepository) : IEntityLockService
{
    private readonly ILockProvider _lockProvider = lockProvider;

    /// <summary>
    /// Dictionary mapping lockable entity types to their lock configuration.
    /// 
    /// To make a new entity type support locking, follow these steps:
    /// 1. Add a new value to <see cref="LockableEntitiesEnum"/> representing the entity.
    ///    Example:
    ///        <c>Product = 1</c>
    /// 2. Create a <see cref="LockableEntityRegistration"/> for the new entity with:
    ///    - <c>entityType</c>: the CLR type of the entity (e.g., <c>typeof(ProductEntity)</c>)
    ///    - <c>ttl</c>: the time-to-live for the lock (e.g., <c>LockConstants.ProductTtl</c>)
    ///    - <c>existsFunc</c>: a function that checks whether a resource exists in the repository.
    ///      It should take <c>resourceId</c>, <c>shopId</c>, and <c>CancellationToken</c> as parameters
    ///      and return a <c>Task&lt;bool&gt;</c>.
    ///      Example:
    ///          <c>(resourceId, shopId, ct) => productRepository.Exists(new GetProductByIdSpecification(resourceId, shopId), ct)</c>
    /// 3. Add the new enum + registration pair to the <c>_registrations</c> dictionary.
    /// 4. Update the <c>LocksController</c> documentation (XML-summary) to include the new entity
    ///    in the list of supported lockable entities, so it appears correctly in Swagger UI.
    /// 
    /// After these steps, the entity will be fully supported by the lock service, including:
    /// - Checking if it is locked by another user
    /// - Acquiring a lock
    /// - Refreshing a lock
    /// - Unlocking
    /// </summary>
    private readonly Dictionary<LockableEntitiesEnum, LockableEntityRegistration> _registrations =
        new()
        {
            {
                LockableEntitiesEnum.Role,
                new LockableEntityRegistration(
                    entityType: typeof(RoleEntity),
                    ttl: LockConstants.RoleTtl,
                    existsFunc: (resourceId, shopId, ct) =>
                        roleRepository.ExistsAsync(new GetRoleByIdandShopIdSpecification(int.Parse(resourceId), shopId), ct))
            }

            // Example
            // {
            //     LockableEntitiesEnum.Product,
            //     new LockableEntityRegistration(
            //         entityType: typeof(ProductEntity),
            //         ttl: LockConstants.ProductTtl,
            //         existsFunc: (resourceId, shopId, ct) =>
            //             productRepository.Exists(
            //                 new GetProductByIdAndShopIdSpecification(int.Parse(resourceId), shopId), ct))
            // }
        };

    public async Task TryLockAsync(
        LockableEntitiesEnum entityType,
        string resourceId,
        int shopId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (!_registrations.TryGetValue(entityType, out var registration))
        {
            throw new NotSupportedException(
                $"No lock configuration found for entity type '{registration!.EntityType.Name}'.");
        }

        bool exists = await registration.ExistsFunc(resourceId, shopId, cancellationToken);
        if (!exists)
        {
            throw new BadRequestException(
                $"Entity of type '{registration!.EntityType.Name}' with ID '{resourceId}' not found in shop {shopId}.");
        }

        bool locked = await _lockProvider.TryLockAsync(
            resourceType: registration!.EntityType.Name,
            resourceId,
            shopId,
            userId,
            registration.Ttl,
            cancellationToken);

        if (!locked)
        {
            throw new ConflictException(
                $"Resource '{registration!.EntityType.Name}' with ID '{resourceId}' is already locked by another user.");
        }
    }

    public async Task RefreshLockAsync(
        LockableEntitiesEnum entityType,
        string resourceId,
        int shopId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (!_registrations.TryGetValue(entityType, out var registration))
        {
            throw new NotSupportedException(
                $"No lock configuration found for entity type '{registration!.EntityType.Name}'.");
        }

        bool exists = await registration.ExistsFunc(resourceId, shopId, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(
                $"Entity of type '{registration!.EntityType.Name}' with ID '{resourceId}' not found in shop {shopId}.");
        }

        bool refreshed = await _lockProvider.RefreshLockAsync(
            resourceType: registration!.EntityType.Name,
            resourceId,
            shopId,
            userId,
            registration.Ttl,
            cancellationToken);

        if (!refreshed)
        {
            throw new ConflictException(
                $"Cannot refresh lock for resource '{registration!.EntityType.Name}' with ID '{resourceId}'. " +
                $"It may be locked by another user or TTL has expired.");
        }
    }

    public async Task UnlockAsync(
        LockableEntitiesEnum entityType,
        string resourceId,
        int shopId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (!_registrations.TryGetValue(entityType, out var registration))
        {
            throw new NotSupportedException(
                $"No lock configuration found for entity type '{registration!.EntityType.Name}'.");
        }

        bool unlocked = await _lockProvider.UnlockAsync(
            resourceType: registration!.EntityType.Name,
            resourceId,
            shopId,
            userId,
            cancellationToken);

        if (!unlocked)
        {
            throw new ConflictException(
                $"Cannot unlock resource '{registration!.EntityType.Name}' with ID '{resourceId}'. " +
                $"It may be locked by another user or lock does not exist.");
        }
    }

    public async Task<bool> IsLockedByAnotherUserAsync(
        LockableEntitiesEnum entityType,
        string resourceId,
        int shopId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        if (!_registrations.TryGetValue(entityType, out var registration))
        {
            throw new NotSupportedException(
                $"No lock configuration found for entity type '{registration!.EntityType.Name}'.");
        }

        var exists = await registration.ExistsFunc(resourceId, shopId, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(
                $"Entity of type '{registration!.EntityType.Name}' with ID '{resourceId}' not found in shop {shopId}.");
        }

        return await _lockProvider.IsLockedByAnotherUserAsync(
            resourceType: registration!.EntityType.Name,
            resourceId,
            shopId,
            currentUserId,
            registration.Ttl,
            cancellationToken);
    }
}