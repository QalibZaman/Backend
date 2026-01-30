using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Commands;

public record LockRoleCommand(int RoleId, Guid UserId, int ShopId) : IRequest<Unit>;

public class LockRoleCommandHandler(IRoleRepository roleRepository) : IRequestHandler<LockRoleCommand, Unit>
{
    public async Task<Unit> Handle(LockRoleCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetRoleByIdandShopIdSpecification(request.RoleId, request.ShopId);
        var roleExists = await roleRepository.Exists(spec, cancellationToken);

        if (!roleExists)
            throw new NotFoundException("A role with this Id and store Id does not exist.");

        var locked = await roleRepository.TryLockAsync(
            request.RoleId,
            request.ShopId,
            request.UserId,
            DateTimeOffset.UtcNow,
            RoleLockConstants.Ttl,
            cancellationToken);

        if (!locked)
            throw new ConflictException("This role is currently being modified. Please try again later.");

        return Unit.Value;
    }
}