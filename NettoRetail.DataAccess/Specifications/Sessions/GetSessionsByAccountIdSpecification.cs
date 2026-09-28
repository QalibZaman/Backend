using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Sessions
{
    public class GetSessionsByAccountIdSpecification : Specification<SessionEntity>
    {
        public GetSessionsByAccountIdSpecification(string id)
            : base(x => x.StaffId.ToString() == id && x.IsActive) { }

    }
}
