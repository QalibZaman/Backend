using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Token;

public class GetBlackListedTokenByJwtSpecification : Specification<BlacklistedTokenEntity>
{
    public GetBlackListedTokenByJwtSpecification(string jwt)
    : base(t => t.Token == jwt) { }
}