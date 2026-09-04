using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Specifications.PasswordHistory
{
    public class GetPasswordHistoryByAccountIdSpecification : Specification<PasswordHistoryEntity>
    {
        public GetPasswordHistoryByAccountIdSpecification(Guid accountId)
       : base(x => x.AccountId == accountId) { }
    }
}
