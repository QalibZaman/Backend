using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;

public class GetBlackListedTokenByJwtSpecification : Specification<BlacklistedTokenEntity>
{
    public GetBlackListedTokenByJwtSpecification(string jwt)
    : base(t => t.Token == jwt) { }
}