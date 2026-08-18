using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Sessions;
public class GetActiveSessionsByAccountIdSpecification(Guid staffId) : Specification<SessionEntity>(s => s.StaffId == staffId && s.ExpiresAt > DateTime.UtcNow && s.IsActive == true)
{
}
