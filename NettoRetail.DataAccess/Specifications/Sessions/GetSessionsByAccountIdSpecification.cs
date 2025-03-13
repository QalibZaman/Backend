using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Sessions;

public class GetSessionsByAccountIdSpecification : Specification<SessionEntity>
{
    public GetSessionsByAccountIdSpecification(Guid accountId)
    : base(s => s.AccountId == accountId) { }
}