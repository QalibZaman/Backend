using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Staff;
public class GetFailedAuthAttemptByAccountIdSpecification : Specification<FailedAuthAttemptEntity>
{
    public GetFailedAuthAttemptByAccountIdSpecification(Guid id)
    : base(x => x.AccountId == id) { }
}
