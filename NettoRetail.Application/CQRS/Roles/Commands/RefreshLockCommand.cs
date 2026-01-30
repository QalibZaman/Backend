using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Commands;

public record RefreshLockCommand(int RoleId, Guid UserId, int ShopId) : IRequest<Unit>;

public class RefreshLockCommandHandler(IRoleRepository roleRepository) : IRequestHandler<RefreshLockCommand, Unit>
{
    public async Task<Unit> Handle(RefreshLockCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetRoleByIdandShopIdSpecification(request.RoleId, request.ShopId);
        var roleExists = await roleRepository.Exists(spec, cancellationToken);

        if (!roleExists)
            throw new NotFoundException("A role with this Id and store Id does not exist.");

        var refreshed = await roleRepository.RefreshLockAsync(
            request.RoleId,
            request.ShopId,
            request.UserId,
            DateTime.UtcNow,
            RoleLockConstants.Ttl,
            cancellationToken);

        if (!refreshed)
            throw new ConflictException("Lock for this role has expired or is owned by another user.");

        return Unit.Value;
    }
}
