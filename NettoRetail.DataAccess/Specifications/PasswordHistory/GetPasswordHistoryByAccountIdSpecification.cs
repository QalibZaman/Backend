using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.PasswordHistory;

public class GetPasswordHistoryByAccountIdSpecification : Specification<PasswordHistoryEntity>
{
    public GetPasswordHistoryByAccountIdSpecification(Guid accountId)
        : base(x => x.AccountId == accountId) { }
}
