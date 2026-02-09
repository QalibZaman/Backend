using MediatR;
using NettoRetail.Application.Models.Request.Lock;
using NettoRetail.Application.Services.IEntityLockService;

namespace NettoRetail.Application.CQRS.Locks.Commands
{
    public record LockEntityCommand(LockRequest LockRequest, int ShopId, Guid UserId) : IRequest<Unit>;

    public class LockEntityCommandHandler(IEntityLockService lockService) : IRequestHandler<LockEntityCommand, Unit>
    {
        public async Task<Unit> Handle(LockEntityCommand request, CancellationToken cancellationToken)
        {
            await lockService.TryLockAsync(
                entityType: request.LockRequest.EntityToLock,
                resourceId: request.LockRequest.EntityId,
                shopId: request.ShopId,
                userId: request.UserId,
                cancellationToken: cancellationToken);

            return Unit.Value;
        }
    }
}
