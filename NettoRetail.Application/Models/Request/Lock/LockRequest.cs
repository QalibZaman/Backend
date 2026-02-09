using NettoRetail.Application.Enums;

namespace NettoRetail.Application.Models.Request.Lock;

public class LockRequest
{
    public LockableEntitiesEnum EntityToLock { get; set; }
    public string EntityId { get; set; }
}
