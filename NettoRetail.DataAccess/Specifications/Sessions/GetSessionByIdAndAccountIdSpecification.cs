using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Sessions
{
    public class GetSessionByIdAndAccountIdSpecification : Specification<SessionEntity>
    {
        public GetSessionByIdAndAccountIdSpecification(Guid id, Guid accountId)
        : base(s => s.Id == id && s.StaffId == accountId && s.IsActive) { }
    }
}
