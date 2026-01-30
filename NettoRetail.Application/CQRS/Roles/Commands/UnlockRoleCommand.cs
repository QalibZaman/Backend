using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Commands;

public record UnlockRoleCommand(int RoleId, Guid UserId, int ShopId) : IRequest<Unit>;

public class UnlockRoleCommandHandler(IRoleRepository roleRepository) : IRequestHandler<UnlockRoleCommand, Unit>
{
    public async Task<Unit> Handle(UnlockRoleCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetRoleByIdandShopIdSpecification(request.RoleId, request.ShopId);
        var roleExists = await roleRepository.Exists(spec, cancellationToken);

        if (!roleExists)
            throw new NotFoundException("A role with this Id and store Id does not exist.");

        var unlocked = await roleRepository.UnlockAsync(
            request.RoleId,
            request.ShopId,
            request.UserId,
            cancellationToken);

        if (!unlocked)
            throw new ConflictException("This role is locked by another user and cannot be unlocked.");

        return Unit.Value;
    }
}