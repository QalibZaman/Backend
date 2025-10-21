using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Sessions;

public class GetSessionsByAccountIdExceptCurrentSpecification : Specification<SessionEntity>
{
    public GetSessionsByAccountIdExceptCurrentSpecification(Guid accountId, string token)
        : base(s => s.AccountId == accountId && s.Jwt != token && s.IsActive) { }
}
