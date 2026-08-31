using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Sessions
{
    public class GetActiveSessionsByAccountIdSpecification : Specification<SessionEntity>
    {
        public GetActiveSessionsByAccountIdSpecification(Guid accountId)
            : base(s => s.StaffId == accountId && s.ExpiresAt > DateTime.UtcNow && s.IsActive == true)
{
}
    }
}
