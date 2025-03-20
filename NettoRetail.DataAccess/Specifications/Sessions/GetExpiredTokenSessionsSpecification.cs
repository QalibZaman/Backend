using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Sessions;

public class GetExpiredTokenSessionsSpecification : Specification<SessionEntity>
{
    public GetExpiredTokenSessionsSpecification()
        : base(s => s.ExpiresAt <= DateTime.UtcNow) { }
}
