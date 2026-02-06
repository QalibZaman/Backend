using MediatR;
using NettoRetail.Application.Models.Request.Lock;
using NettoRetail.Application.Services.IEntityLockService;

namespace NettoRetail.Application.CQRS.Locks.Commands;

public record UnlockEntityCommand(LockRequest LockRequest, int ShopId, Guid UserId) : IRequest<Unit>;

public class UnlockEntityCommandHandler(IEntityLockService lockService) : IRequestHandler<UnlockEntityCommand, Unit>
{
    public async Task<Unit> Handle(UnlockEntityCommand request, CancellationToken cancellationToken)
    {
        await lockService.UnlockAsync(
            entityType: request.LockRequest.EntityToLock,
            resourceId: request.LockRequest.EntityId,
            shopId: request.ShopId,
            userId: request.UserId,
            cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
