using MediatR;
using NettoRetail.Application.Models.Request.Lock;
using NettoRetail.Application.Services.IEntityLockService;

namespace NettoRetail.Application.CQRS.Locks.Queries;

public record IsEntityLockedByAnotherUserCommand(LockRequest LockRequest, int ShopId, Guid UserId) : IRequest<bool>;
public class IsEntityBlockedByAnotherUserCommandHandler(IEntityLockService lockService) : IRequestHandler<IsEntityLockedByAnotherUserCommand, bool>
{
    public async Task<bool> Handle(IsEntityLockedByAnotherUserCommand request, CancellationToken cancellationToken)
    {
        var result = await lockService.IsLockedByAnotherUserAsync(
            request.LockRequest.EntityToLock,
            request.LockRequest.EntityId,
            request.ShopId,
            request.UserId,
            cancellationToken);

        return result;
    }
}
