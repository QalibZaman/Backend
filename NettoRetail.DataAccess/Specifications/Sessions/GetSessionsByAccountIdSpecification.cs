using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Sessions
{
    public class GetSessionsByAccountIdSpecification : Specification<SessionEntity>
    {
        public GetSessionsByAccountIdSpecification(string id)
            : base(x => x.AccountId.ToString() == id && x.IsActive) { }

    }
}
